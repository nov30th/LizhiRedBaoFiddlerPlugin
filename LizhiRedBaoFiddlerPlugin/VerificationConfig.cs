using System;
using System.Management;
using Microsoft.Win32;


/* Licence Checker
 * @Author:Zhongjie.Qiu@Sumscope.Com
 */
namespace LizhiRedBaoFiddlerPlugin
{
    public static class VerificationConfig
    {
        public static string DefaultSerialNumberFieldName { get; } = "SerialNumber";

        public static VerificationResult CheckVerificationStatus()
        {
            return CheckVerificationStatus(string.Empty);
        }

        public static bool DoVerificationProcess()
        {
            try
            {
                var verificationResult = CheckVerificationStatus();
                if (verificationResult != VerificationResult.NORMAL)
                {
                    //Console.WriteLine(@"Press any key to exit...");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static VerificationResult CheckVerificationStatus(string sericalNumber)
        {
            try
            {
                if (sericalNumber == "-1")
                    return VerificationResult.NO_INFORMATION;

                /* 比较CPUid */
                var cpuId = GetSoftEndDateAllCpuId(1, sericalNumber); //从注册表读取CPUid
                var cpuIdThis = GetCpuIdHashedString(); //获取本机CPUId         
                if (cpuId != cpuIdThis)
                    return VerificationResult.INFORMATION_ERROR;

                /* 比较时间 */
                var nowDate = GetNowDate();
                var expirationDate = GetExpirationDate(sericalNumber);

                if (string.IsNullOrEmpty(expirationDate))
                    return VerificationResult.INFORMATION_ERROR;

                if (Convert.ToInt32(expirationDate) - Convert.ToInt32(nowDate) < 0)
                    return VerificationResult.EXPIRED;

                return VerificationResult.NORMAL;
            }
            catch
            {
                return VerificationResult.INFORMATION_ERROR;
            }
        }


        /// <summary>
        ///     Gets expiration date from register
        ///     This function including CPUID verification, which will returns null if register CPUID differents as local
        ///     computer's.
        /// </summary>
        /// <param name="sericalNumberHashedString">sericalNumber key in register, leave black if default.</param>
        /// <returns></returns>
        private static string GetExpirationDate(string sericalNumberHashedString)
        {
            /* 比较CPUid */
            var cpuId = GetSoftEndDateAllCpuId(1, sericalNumberHashedString);
            var cpuIdThis = GetCpuIdHashedString(); //获取本机CPUId
            if (cpuId != cpuIdThis)
                return string.Empty;
            var endDate = GetSoftEndDateAllCpuId(0, sericalNumberHashedString);
            return endDate;
        }


        /*CPUid*/
        public static string GetCpuIdHashedString(string customHashedString = "")
        {
            if (!string.IsNullOrEmpty(customHashedString))
                return customHashedString;
            var mc = new ManagementClass("Win32_Processor");
            var moc = mc.GetInstances();

            string strCpuId = null;
            foreach (var o in moc)
            {
                var mo = (ManagementObject) o;
                strCpuId = mo.Properties["ProcessorId"].Value.ToString();
                //strCpuId = Md5Util.CalculateMd5Hash(strCpuId);
                break;
            }
            return strCpuId;
        }

        /*当前时间*/
        public static string GetNowDate()
        {
            var nowDate =
                DateTime.Now.ToString("yyyyMMdd"); //.Year + DateTime.Now.Month + DateTime.Now.Day).ToString();
            //     DateTime date = Convert.ToDateTime(NowDate, "yyyy/MM/dd");
            return nowDate;
        }

        /* 生成明文序列号 */
        public static string GetSerialNumber(string yyyyMMddDate, string customCpuidHashedString = "")
        {
            if (!string.IsNullOrEmpty(customCpuidHashedString))
                return customCpuidHashedString + "-" + yyyyMMddDate;
            var serialNumber = GetCpuIdHashedString() + "-" + yyyyMMddDate;
            return serialNumber;
        }

        public static string GetEncryptedSerialNumber(string yyyyMMddDate, string customCpuidHashedString = "")
        {
            return DesUtil.Encrypt(GetSerialNumber(yyyyMMddDate, customCpuidHashedString));
        }


        /* 
         * i=1 得到 CUP 的id 
         * i=0 得到上次或者 开始时间 
         */
        public static string GetSoftEndDateAllCpuId(int i, string serialNumber)
        {
            if (!serialNumber.Contains("-") || serialNumber.StartsWith("-") || serialNumber.EndsWith("-"))
                return string.Empty;

            switch (i)
            {
                case 1:
                    var cupId = serialNumber.Substring(0, serialNumber.LastIndexOf("-", StringComparison.Ordinal));
                    return cupId.Trim();
                case 0:
                    var dateTime = serialNumber.Substring(serialNumber.LastIndexOf("-", StringComparison.Ordinal) + 1);
                    return dateTime.Trim();
                default:
                    return string.Empty;
            }
        }
    }
}