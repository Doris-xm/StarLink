from math import floor, pi, log, tan, atan, exp
from threading import Thread, Lock
import urllib.request as ur
import PIL.Image as pil
from PIL import Image, ImageDraw, ImageFont
import datetime
import io
import os

MAP_URLS = {
    "google": "http://mt2.google.cn/vt/lyrs={style}&hl=zh-CN&gl=CN&src=app&x={x}&y={y}&z={z}",
    "amap": "https://webst01.is.autonavi.com/appmaptile?x={x}&y={y}&z={z}&style=6",
}

COUNT=0
mutex=Lock()

def wgs_to_macator(x, y):
    y = 85.0511287798 if y > 85.0511287798 else y
    y = -85.0511287798 if y < -85.0511287798 else y

    x2 = x * 20037508.34 / 180
    y2 = log(tan((90 + y) * pi / 360)) / (pi / 180)
    y2 = y2 * 20037508.34 / 180
    return x2, y2

def mecator_to_wgs(x, y):
    x2 = x / 20037508.34 * 180
    y2 = y / 20037508.34 * 180
    y2 = 180 / pi * (2 * atan(exp(y2 * pi / 180)) - pi / 2)
    return x2, y2
    
def wgs84_to_tile(j, w, z):
    isnum = lambda x: isinstance(x, int) or isinstance(x, float)
    if not(isnum(j) and isnum(w)):
        raise TypeError("j and w must be int or float!")

    if not isinstance(z, int) or z < 0 or z > 22:
        raise TypeError("z must be int and between 0 to 22.")

    if j < 0:
        j = 180 + j
    else:
        j += 180
    j /= 360  # make j to (0,1)

    w = 85.0511287798 if w > 85.0511287798 else w
    w = -85.0511287798 if w < -85.0511287798 else w
    w = log(tan((90 + w) * pi / 360)) / (pi / 180)
    w /= 180  # make w to (-1,1)
    w = 1 - (w + 1) / 2  # make w to (0,1) and left top is 0-point

    num = 2**z
    x = floor(j * num)
    y = floor(w * num)
    return x, y


def tileframe_to_mecatorframe(zb):
    inx, iny =zb["LT"]   #left top
    inx2,iny2=zb["RB"] #right bottom
    length = 20037508.3427892
    sum = 2**zb["z"]
    LTx = inx / sum * length * 2 - length
    LTy = -(iny / sum * length * 2) + length

    RBx = (inx2 + 1) / sum * length * 2 - length
    RBy = -((iny2 + 1) / sum * length * 2) + length

    # LT=left top,RB=right buttom
    res = {'LT': (LTx, LTy), 'RB': (RBx, RBy),
           'LB': (LTx, RBy), 'RT': (RBx, LTy)}
    return res

def tileframe_to_pixframe(zb):
    out={}
    width=(zb["RT"][0]-zb["LT"][0]+1)*256
    height=(zb["LB"][1]-zb["LT"][1]+1)*256
    out["LT"]=(0,0)
    out["RT"]=(width,0)
    out["LB"]=(0,-height)
    out["RB"]=(width,-height)
    return out

class Downloader(Thread):
    # multiple threads downloader
    def __init__(self,index,count,urls,datas,update):
        super().__init__()
        self.urls=urls
        self.datas=datas
        self.index=index
        self.count=count
        self.update=update

    def download(self,url):
        HEADERS = {'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36'}
        header = ur.Request(url,headers=HEADERS)
        err=0
        while(err<3):
            try:
                data = ur.urlopen(header).read()
            except:
                err+=1
            else:
                return data
        raise Exception("Bad network link.")

    def run(self):
        for i,url in enumerate(self.urls):
            if i%self.count != self.index:
                continue
            self.datas[i]=self.download(url)
            if mutex.acquire():
                self.update()
                mutex.release()

def geturl(source, x, y, z):
    if source == 'google':
        furl = MAP_URLS["google"].format(x=x, y=y, z=z)
    elif source == 'amap':
        furl = MAP_URLS["amap"].format(x=x, y=y, z=z)
    return furl

def downpics(urls,multi=10):
    def makeupdate(s):
        def up():
            global COUNT
            COUNT+=1
            print("\b"*45,end='')
            print("DownLoading ... [{0}/{1}]".format(COUNT,s),end='')
        return up
    url_len=len(urls)
    datas=[None] * url_len
    if multi <1 or multi >20 or not isinstance(multi,int):
        raise Exception("multi of Downloader shuold be int and between 1 to 20.")
    tasks=[Downloader(i,multi,urls,datas,makeupdate(url_len)) for i in range(multi)]
    for i in tasks:
        i.start()
    for i in tasks:
        i.join()
    return datas


def testTile(x1, y1, x2, y2, z, source='google', outfile="MAP_OUT.png", style='s'):
    pos1x, pos1y = wgs84_to_tile(x1, y1, z)
    pos2x, pos2y = wgs84_to_tile(x2, y2, z)
    lenx = pos2x - pos1x + 1
    leny = pos2y - pos1y + 1
    print("Total number: {x} X {y}".format(x=lenx, y=leny))

    urls = [geturl(source, i, j, z) for j in range(pos1y, pos1y + leny) for i in range(pos1x, pos1x + lenx)]

    datas = downpics(urls)

    print("\nDownload Finished! Pics Mergeing......")
    outpic = pil.new('RGB', (lenx * 256, leny * 256))  # Set image mode to RGB

    for i, data in enumerate(datas):
        picio = io.BytesIO(data)
        small_pic = pil.open(picio)

        y, x = i // lenx, i % lenx
        outpic.paste(small_pic, (x * 256, y * 256))

    print('Pics Merged! Exporting......')
    outpic.save(outfile)
    print('Exported to file!')
    return {"LT":(pos1x,pos1y),"RT":(pos2x,pos1y),"LB":(pos1x,pos2y),"RB":(pos2x,pos2y),"z":z}
    

def getTile(x1, y1, x2, y2, z = 16, source='amap'):
    pos1x, pos1y = wgs84_to_tile(x1, y1, z)
    pos2x, pos2y = wgs84_to_tile(x2, y2, z)
    lenx = pos2x - pos1x + 1
    leny = pos2y - pos1y + 1
    while lenx * leny > 20:
        z -= 1
        pos1x, pos1y = wgs84_to_tile(x1, y1, z)
        pos2x, pos2y = wgs84_to_tile(x2, y2, z)
        lenx = pos2x - pos1x + 1
        leny = pos2y - pos1y + 1

    print("Total number: {x} X {y}".format(x=lenx, y=leny))

    urls = [geturl(source, i, j, z) for j in range(pos1y, pos1y + leny) for i in range(pos1x, pos1x + lenx)]

    datas = downpics(urls)

    print("\nDownload Finished! Pics Mergeing......")
    outpic = pil.new('RGB', (lenx * 256, leny * 256))  # Set image mode to RGB

    for i, data in enumerate(datas):
        picio = io.BytesIO(data)
        small_pic = pil.open(picio)

        y, x = i // lenx, i % lenx
        outpic.paste(small_pic, (x * 256, y * 256))
    
    # Convert PIL.Image to bytes
    with io.BytesIO() as output:
        outpic.save(output, format='PNG')
        image_data = output.getvalue()

    return image_data

def getPhoto(x1, y1):
    img_store = []
    if abs(x1 - 31.15762) < 0.5 and abs(y1 - 121.7862) < 0.5:
        dir = 'utils/pd'
    elif abs(x1 - 23.38957) < 0.5 and abs(y1 - 113.2979) < 0.5:
        dir = 'utils/by'
    elif abs(x1 - 40.64318) < 0.5 and abs(y1 - -72.22293) < 0.5:
        dir = 'utils/knd'
    elif abs(x1 - 38.32444) < 0.5 and abs(y1 - 106.3794) < 0.5:
        dir = 'utils/hd'
    elif abs(x1 - 19.943456) < 0.5 and abs(y1 - 110.465511) < 0.5:
        dir = 'utils/ml'
    # get all photos from direction dir
    for root, dirs, files in os.walk(dir):
        # store img in a tuple
        for file in files:
            with open(os.path.join(root, file), 'rb') as f:
                img_store.append((file, f.read()))
    return img_store

def addTimestamp(imagebytse):
    image = Image.open(io.BytesIO(imagebytse))
    width, height = image.size
    draw = ImageDraw.Draw(image)
    font = ImageFont.truetype('Gubbi', 40)
    current_time = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')
    print(current_time)
    text_width, text_height = draw.textsize(current_time, font)
    margin = 10
    x = margin
    y = margin
    # yellow
    text_color = (255, 255, 0)

    draw.text((x, y), current_time, text_color)
    modified_image = io.BytesIO()
    image.save(modified_image, format='PNG')
    return modified_image.getvalue()

if __name__ == '__main__':
    img_store = getPhoto(31.15762, 121.7862)
    for img in img_store:
        with open (img[0], 'wb') as f:
            f.write(img[1])