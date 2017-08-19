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


        private string merchant_private_key = "MIICXAIBAAKBgQCszxwBKjCHOHKFBQcPu+hcs5yv6w5QggiUJ6rdXI38VV5B9yuwx/lDYMLirVKGtESv+pPk813n0bGMez7Vdm2e34QVyR/9RNea4Aklv2PSyv61jPewquwt2EGQ2Mk+853zGjmLb5uEL8Z8y716JrMy1HxFteAEqCA2A7HLyJdaWwIDAQABAoGADXP6LCUKrhw43h4sFI9+YWkiM5fK/32ACXilFqKT8yb6NYx2fEa1IwevZFI18IKsLj8FsHc5wkhS2CroE1oq4mihfGuAPNCvTAV4QD313mkIP5QIZsMPaZX1oL9YLkhkhNyKI6C8Lffys5wOCyX1GqM753D3cHPB6n269zY0dyECQQDbIlqunbVDsx632bL1HzvM6IL6I+twjRKz82W1tL9E5REL4VzPudAVL3T9Y/oKMi3IOcE4HqwtIzE8PpOQXPE5AkEAyeGgwvfg/weMRnptKOUivSAaEscvurbILnr9oOA70LOaDMn5Sxfb1lEBpYX/KG7tJZ81q6wzSjLLmIMg4QmsMwJAdrvYksCVFMebH1bv5m00A8UAIvUPfv6RYbvCIoB7GqNbZyqHFW7C1pfONfXT525k7BaPIQ9Nj2+AH/pwDkqt0QJADiIJixynV7NDkruHYNGJuQvCR4ZCRSP+p6JclyKbjWTFaBfLqAInlb1eDCRxVHdPis62hyoq/QrJTggACUEQGQJBANfXCgop3ADabZx9f+fETwjiDQVKXe73KbSM4GrNNApbtjA+p1iP9RwoRTNvvhW2Lm9Fjm9LOAM2dMQ4XiekFfk=";

        private string alipay_public_key = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCszxwBKjCHOHKFBQcPu+hcs5yv6w5QggiUJ6rdXI38VV5B9yuwx/lDYMLirVKGtESv+pPk813n0bGMez7Vdm2e34QVyR/9RNea4Aklv2PSyv61jPewquwt2EGQ2Mk+853zGjmLb5uEL8Z8y716JrMy1HxFteAEqCA2A7HLyJdaWwIDAQAB";
        /// <summary>
        /// 支付宝支付
        /// </summary>
        /// <returns></returns>
        public void doPost()
        {
            IAopClient client = new DefaultAopClient("http://openapi.alipaydev.com/gateway.do",
                "2017081108144704",//支付宝分配给开发者的应用ID
                merchant_private_key,
                "json",//仅支持JSON
                "1.0", //调用的接口版本，固定为：1.0
                "RSA2",//商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
                alipay_public_key,
                "GBK",
                false);
            AlipayTradeWapPayRequest request = new AlipayTradeWapPayRequest();
            request.SetNotifyUrl(string.Empty);//设置异步通知地址
            request.SetReturnUrl(string.Empty);//设置同步通知地址
            JObject jobParams = new JObject();
            jobParams.Add(new JProperty("body", "测试订单"));//对一笔交易的具体描述信息
            jobParams.Add(new JProperty("subject", "测试标题"));//商品的标题/交易标题/订单标题/订单关键字等。
            jobParams.Add(new JProperty("out_trade_no", DateTime.Now.ToString("yyyyMMddHHmmssfff")));//商户网站唯一订单号
            jobParams.Add(new JProperty("timeout_express", "2h"));//订单失效时间
            jobParams.Add(new JProperty("total_amount", "0.01"));//订单总金额，单位为元，精确到小数点后两位，取值范围[0.01,100000000]
            jobParams.Add(new JProperty("product_code", "QUICK_WAP_WAY"));//销售产品码，商家和支付宝签约的产品码。该产品请填写固定值：QUICK_WAP_WAY
            //jobParams.Add(new JProperty("quit_url", "QUICK_WAP_WAY"));
            request.BizContent = jobParams.ToString();
            AlipayTradeWapPayResponse response = client.pageExecute(request);
            string form = response.Body;
            Response.Write(form);
        }
    }
}