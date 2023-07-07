from satellite import SatelliteInfo

IDs = [
    25544,
    44713,
    44717,
    44752,
    44718,
    44725,
    44738,
    44757,
    44920,
    44926,
    45365,
    45379,
]
# for test
def print_satellite_info(satellite_id):
    # 创建SatelliteInfo对象
    for id in IDs:
        satellite_info = SatelliteInfo(id)
        satellite, stamp = satellite_info.get_satellite_info()
        print(str(satellite["id"])+"  "+ satellite["name"])
    # satellite_info = SatelliteInfo(satellite_id)
    # satellite = satellite_info.get_satellite_info()

    # json 格式示例：
    # "sats": [
    #         {
    #             "id": 25544,
    #             "name": "0-A",
    #             "oname": "ISS",
    #             "lat": 38.3524, 纬度
    #             "lng": 79.4576, 经度
    #             "alt": 416.8,   海拔
    #             "alt2": 415.5,  海拔（另一种指标下的）
    #             "p": 413.1,     轨道高度？
    #             "lat2": 25.1346,
    #             "lng2": 95.031,
    #             "illum": 1,     照明状态，是否处于阳光照射下
    #             "raan": 295.5138,升交点赤经
    #             "age": 0.3，    卫星年龄
    #         },
    # if satellite:
    #     print("Satellite Info:")
    #     print(satellite)
    # else:
    #     print("Satellite not found.")



    # # 计算偏角
    # azimuth = satellite_info.calculate_azimuth()
    # print("Azimuth:", azimuth)



# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    print_satellite_info(25544)

