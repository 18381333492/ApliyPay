using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Aop;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Domain;
using Aop.Api.Util;
using Aop.Api.Parser;

namespace Web.Controllers
{
    public class PayController : Controller
    {
        // GET: Pay
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <returns></returns>
        public ActionResult BookOrder()
        {
            //获取用户的终端IP
            string ip = Request.UserHostAddress;
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"]!=null)
             ip =  Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0];

            JObject param = new JObject();

            param.Add(new JProperty("body", "测试订单"));
            param.Add(new JProperty("out_trade_no", DateTime.Now.ToString("yyyyMMddHHmmssfff")));
            param.Add(new JProperty("total_fee", "1"));
            param.Add(new JProperty("spbill_create_ip", ip));
            param.Add(new JProperty("notify_url", "http://t17m267950.imwork.net/Pay/notify"));
            param.Add(new JProperty("product_id", "1"));
            
            string sUrl= "http://t17m267950.imwork.net/Pay/BookOrder";

            string res = HttpPost(sUrl, param.ToString());
            return Content(res);
        }

        /// <summary>
        /// Post请求微信系统
        /// </summary>
        /// <param name="sUrl">请求的链接</param>
        /// <param name="PostData">请求的参数</param>
        /// <returns></returns>
        public  string HttpPost(string sUrl, string PostData)
        {
            byte[] bPostData = System.Text.Encoding.UTF8.GetBytes(PostData);
            string sResult = string.Empty;
            try
            {
                HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sUrl);
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.Timeout = 30000;
                webRequest.Method = "POST";
                webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                if (bPostData != null)
                {
                    Stream postDataStream = webRequest.GetRequestStream();
                    postDataStream.Write(bPostData, 0, bPostData.Length);
                }
                HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();
                if (webResponse.ContentEncoding.ToLower() == "gzip")//如果使用了GZip则先解压
                {
                    using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                    {
                        using (var zipStream =
                            new System.IO.Compression.GZipStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                        {
                            using (StreamReader sr = new System.IO.StreamReader(zipStream, Encoding.GetEncoding(webResponse.CharacterSet)))
                            {
                                sResult = sr.ReadToEnd();
                            }
                        }
                    }
                }
                else if (webResponse.ContentEncoding.ToLower() == "deflate")//如果使用了deflate则先解压
                {
                    using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                    {
                        using (var zipStream =
                            new System.IO.Compression.DeflateStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                        {
                            using (StreamReader sr = new System.IO.StreamReader(zipStream, Encoding.GetEncoding(webResponse.CharacterSet)))
                            {
                                sResult = sr.ReadToEnd();
                            }
                        }
                    }
                }
                else
                {
                    using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8))
                        {
                            sResult = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return sResult;
        }


        private string merchant_private_key = "MIIEowIBAAKCAQEAyar5QMTDtAarKX9tcx0IluaSWezSc7GHqvcV8Yv+sEpFZCQZUbHsN7ssJXJXUFhsgCqvPTLDlmO0ns4PdNFSwMT2Q1JAT1dXSXEVvH/OorRA73L2oFEsrTKACN1xvrlsnF4OmH/z2/zCLjyQpAuvxXrfF6tU9uJyDB9iCCOuFJBgmD+N1ALosto6aALRKlMmF3ucL/XUVPjQY9bxqcDzGGkZnZxUeHLLF+9yP6JG814w+moy2PbSMvo+Vm4adDVi+aLVjbD3HOPhblxC4NXjc9MWxCThsCmRUDmkSVTcJNR8Q+DefR7604fzq8IoXLniDm0oO0Yi3qQnUd20PuMK8QIDAQABAoIBAHfYnoXqKS98Yw2nR8EIOQmMft7oCW1tzGVCr4y7mKDlknVfqphNN0creaHLYK5Dzj8gnsGswGVIXZeed7sBhr8+jecWI1fDXQEtLjC2d3Nj0c87L+u4Mee/wi0ChM1GXpBSqTPhnmdWv4NAxOhodY3TZm8nh7esfQBNSjHyGkrnMA3h9PtTEJd92KifPUYuvzHD2KOqzhHJboZ8Ih/lRh5uWCEp7366Mc3q2RafpLVxRjchPfGeU7PPidviFrnhN0BjaAsawPzPCpfn04zvS96dNbuJKLOZ0sMOHhxNLphWsk68nVUNytbmS8lpr2cKMPdg/QoiwGvhrNNOyxFTxAECgYEA6JTNlJBqfGmQkMtto09VJOhhO0r67nMd+fKdxBP+F5WVzNqoTBycH0HoIz6XYIsEubyAVMRNqZvQd9uvl7m3pgnGtEIj5TKB9WkpZtMe/k87APCeq+rZLjvt1bax5/Y9BQZwB3O0x2jAtVHZc2sO44ybQnKNAzTGvBvSbiQ8dEECgYEA3flSmfq/A4qTJ5gMSi60zFpn6v2UfDkv/UGwfOAWH+RrNKDq6wg2PVsRrkpOsS9oSkyiVtkWa4XVx9m34zo4APvOKoUyqfI3petTDtGV1zBhBvkOl6bCVAVxHUg6VN4wObaq86UR8FdfsauikTHxFscPgP+4mcr+DJyymDUEKrECgYEAqVgfT7rPLgMXFbZo/+21iwgAM9HmX1RGUUWMBcagzb9GsT/MJo72RfQQ+AiM4+iU6kAMGKxN997RrVOxyIGa7DRWD83QoQNjiLKnSI0UFgrOZWLNxVNcCsPr6h3573FlAJGtZF+lE0R8fAk6kUU0NA6exYTuk5UL1s9TKosL0YECgYB52cvGSydgQknViln0vv7wzxAMp3dDWgFF/TFs23ZJu5I+KbfLnY5oz/08t/3KtkOBxd+33SO5kpZwRsvzKJplr9TU8pmFQTnbEvtdPyAKKLyan02rYhd7GCGn+WZMAExo4iWl6g+W59/YIGf1XH0EC/Iu1jH3+r7LHZnMhA3tgQKBgAENUaxBNuWdPY/LMac1M6kEkxx377Vi41EBZOHpNHxiusN7q5CeW6/EvxodwMvBJaJXfSNSnIBth44PH06SmP6AGSp4cong7xpR3v3BGf3MI06Y4oNcgXv1gXg2nSgMH2y1g8PkE9lhVrdwKNGBusMV0yaDQCXfvv0Zaa3d1MWu";

        private string alipay_public_key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl4cdAY6ll1ZJyGLNUr/Yh+W2lZnPCzsGYzx9Qavg6PTrFK8BSZB6Pm9wNrhQcgMhOaQTqV8xMOPmtJMEEdcy3zKul7eVfJRCuQBwQ8i+DP2IMxa05+UjvBSyP5DMip5B/HMpqxl7Xs7cbqzH9G4/lrWo6nHvWhy7aO7ICu/+oxPPFadUj4vI0ZqA39falmlBFNk7lQDWXPxTt/gHkyPsL9zQkaYTMd6hPR4zsFJLqvm+Zl21fXWn0qyeOdlwRGBud8bdfif1jCOzoDlyZvWHw2eYzEgV99udBq6o7Iv+Z1f2upXYitLqv5ISOuicu9te/a7ePZOkLIbD7YCaSmQbBQIDAQAB";
        /// <summary>
        /// 支付宝支付
        /// </summary>
        /// <returns></returns>
        public ActionResult doPost()
        {
            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do",
                "2017081108144704",    //"2017081108144704",//支付宝分配给开发者的应用ID
                merchant_private_key,
                "json",//仅支持JSON
                "1.0", //调用的接口版本，固定为：1.0
                "RSA2",//商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
                alipay_public_key,
                "utf-8",
                false);
            AlipayTradeWapPayRequest request = new AlipayTradeWapPayRequest();

            AlipayTradeWapPayModel model = new AlipayTradeWapPayModel();
           // model.Body= "测试订单";////对一笔交易的具体描述信息
            model.OutTradeNo= DateTime.Now.ToString("yyyyMMddHHmmssfff");
            model.ProductCode = "QUICK_WAP_WAY";
            model.Subject = "测试订单";//商品的标题/交易标题/订单标题/订单关键字等。
            model.TimeoutExpress = "2h";////订单失效时间
            model.TotalAmount = "0.01";//订单总金额，单位为元，精确到小数点后两位，取值范围[0.01,100000000]
            request.SetBizModel(model);
            request.SetNotifyUrl("http://t17m267950.imwork.net/AlipayNotify.ashx");//设置异步通知地址
            request.SetReturnUrl("http://t17m267950.imwork.net/Pay/Success");//设置同步通知地址
            string form = client.pageExecute(request).Body;
           // Response.Write(form);
            return Json(new { form = form });
        }


        /// <summary>
        /// 支付宝异步通知
        /// </summary>
        public void Notify()
        {
            byte[] bNetStream =Request.BinaryRead(Request.ContentLength);
            string RequestParams = System.Text.Encoding.UTF8.GetString(bNetStream);
            RequestParams = HttpUtility.UrlDecode(RequestParams);
            var DicData = new AopDictionary();

            foreach (string key in Request.Form.Keys)
            {
                DicData.Add(key, Request.Form[key]);
            }

            // var Apikey = @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl4cdAY6ll1ZJyGLNUr/Yh+W2lZnPCzsGYzx9Qavg6PTrFK8BSZB6Pm9wNrhQcgMhOaQTqV8xMOPmtJMEEdcy3zKul7eVfJRCuQBwQ8i+DP2IMxa05+UjvBSyP5DMip5B/HMpqxl7Xs7cbqzH9G4/lrWo6nHvWhy7aO7ICu/+oxPPFadUj4vI0ZqA39falmlBFNk7lQDWXPxTt/gHkyPsL9zQkaYTMd6hPR4zsFJLqvm+Zl21fXWn0qyeOdlwRGBud8bdfif1jCOzoDlyZvWHw2eYzEgV99udBq6o7Iv+Z1f2upXYitLqv5ISOuicu9te/a7ePZOkLIbD7YCaSmQbBQIDAQAB";
            //foreach(string key in Request.QueryString.Keys)
            //{
            //    DicData.Add(key, Request.QueryString[key]);
            //}

            //验证签名
            if (AlipaySignature.RSACheckV1(DicData, alipay_public_key, DicData["charset"], DicData["sign_type"], false))
            {
                if (DicData["app_id"] == "2017081108144704")
                {
                    if (DicData["trade_status"] == "TRADE_SUCCESS")
                    {//支付成功
                        
                    }
                    Response.Clear();
                    Response.Write("success");
                }
                else
                {//通知异常(假通知)
                   
                }
            }
        }



        /// <summary>
        /// 支付宝支付状态查询
        /// </summary>
        public void PayQuery()
        {
            string out_trade_no = "20170821141701640";
            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do",
             "2017081108144704",
            merchant_private_key,
                "json", 
                "1.0",
                "RSA2",
               alipay_public_key,
               "utf-8",
                false);
            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            JObject job = new JObject();
            job.Add(new JProperty("out_trade_no", out_trade_no));
            request.BizContent = job.ToString();
            AlipayTradeQueryResponse response = client.Execute(request);
            JObject resultJosn =JObject.Parse(response.Body);
            if (resultJosn["trade_status"].ToString() == "TRADE_SUCCESS")
            {//支付成功

            }
            else
            {
                //if (resultJosn["trade_status"].ToString() == "WAIT_BUYER_PAY")//交易创建，等待买家付款
                //if (resultJosn["trade_status"].ToString() == "TRADE_CLOSED")//未付款交易超时关闭，或支付完成后全额退款
                //if (resultJosn["trade_status"].ToString() == "TRADE_FINISHED")//交易结束，不可退款

            }
        }



        /// <summary>
        /// 支付宝申请退款
        /// </summary>
        public void PayRefund()
        {
            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do",
                "2017081108144704",
              merchant_private_key, 
                "json", 
                "1.0",
                "RSA2",
                alipay_public_key,
                "utf-8",
                false);
            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();

            AlipayTradeRefundModel model = new AlipayTradeRefundModel();

            model.OutTradeNo = "20170821141701640";
            model.RefundAmount = "0.01";
            model.RefundReason = "正常退款";

            request.SetBizModel(model);

            //request.BizContent = "{" +
            //"\"out_trade_no\":\"20150320010101001\"," +
            //"\"trade_no\":\"2014112611001004680073956707\"," +
            //"\"refund_amount\":200.12," +
            //"\"refund_reason\":\"正常退款\"," +
            //"\"out_request_no\":\"HZ01RF001\"," +
            //"\"operator_id\":\"OP001\"," +
            //"\"store_id\":\"NJ_S_001\"," +
            //"\"terminal_id\":\"NJ_T_001\"" +
            //"  }";
            AlipayTradeRefundResponse response = client.Execute(request);
            string result = response.Body;

            var paramsData = JObject.Parse(result);


                //AopJsonParser<AopResponse>
        }



        /// <summary>
        /// 同步回调地址
        /// </summary>
        /// <returns></returns>
        public ActionResult Success()
        {
            return View();
        }
    }
}