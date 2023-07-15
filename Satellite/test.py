from utils.tile import getTile
from utils.tile import getPhoto


img_store = getPhoto(31.15762, 121.7862)
for img in img_store:
    with open(img[0], 'wb') as f:
        f.write(img[1])