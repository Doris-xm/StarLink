import math


class TimeConverter:
    RADIUS = 6378.135
    SEC_IN_DAY = 86400.0
    SIDERAL_ROTATION = 1.00273790934

    def __init__(self, time):
        coef1 = 365.25
        coef2 = 30.6001
        coef3 = 14.0
        coef4 = 1720994.5

        self.year = time.year
        self.day = time.timetuple().tm_yday + (
                    time.hour + ((time.minute + ((time.second + (time.microsecond / 1000000.0)) / 60.0)) / 60.0)) / 24.0
        year = self.year
        year -= 1
        self.date = int(coef1 * year) + int(coef2 * coef3) + coef4 + (
                    2.0 - (year / 100.0) + (year / 100.0 / 4.0)) + self.day

    # 计算特定地点的本地平均恒星时（Local Mean Sidereal Time，LMST）：将经度位置考虑在内
    def to_lms_time(self, longitude):
        return (self.to_gms_time() + longitude) % (2 * math.pi)

    # 格林尼治平均恒星时GMST：使用当前日期来计算与标准参考日期（2451545.0）的时间差
    def to_gms_time(self):
        ut = (self.date + 0.5) % 1.0
        tu = ((self.date - 2451545.0) - ut) / 36525.0
        gmst = 24110.54841 + tu * (8640184.812866 + tu * (0.093104 - tu * 6.2e-06))
        gmst = (gmst + TimeConverter.SEC_IN_DAY * TimeConverter.SIDERAL_ROTATION * ut) % TimeConverter.SEC_IN_DAY
        if gmst < 0.0:
            gmst += TimeConverter.SEC_IN_DAY

        return 2 * math.pi * (gmst / TimeConverter.SEC_IN_DAY)
