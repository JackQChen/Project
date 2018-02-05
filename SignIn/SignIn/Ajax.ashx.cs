using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Services;
using DataAccess;
using WeChatFramework;

namespace SignIn
{
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Ajax : IHttpHandler
    {
        static InvokeCache _instance;

        static InvokeCache instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InvokeCache();
                    _instance.dicConstructor = new Dictionary<string, ConstructorInfo>();
                    _instance.dicMethod = new Dictionary<string, MethodInfo>();
                    ConnectionList.AddConnection("Main",
                        Config.AppSettings["DatabaseType"],
                        Config.AppSettings["ConnectionString"]);
                }
                return _instance;
            }
        }

        //Type,Dll|Method此类格式
        //参数用|分隔
        public void ProcessRequest(HttpContext context)
        {
            string strReturn = "";
            try
            {
                var para = HttpUtility.UrlDecode(context.Request.Form.ToString()).ToJsonObject();
                if (para == null)
                {
                    throw new Exception("请求参数无效，请核对参数");
                }
                string strInvoke = para.ToJsonProperty("invoke");
                string strType = strInvoke.Split(',')[0],
                    strDll = strInvoke.Split(',')[1].Split('|')[0],
                    strMethod = strInvoke.Split('|')[1];
                lock (instance.dicConstructor)
                {
                    if (!instance.dicConstructor.ContainsKey(strType + strDll))
                    {
                        Type tp = Assembly.LoadFile(System.AppDomain.CurrentDomain.BaseDirectory
                          + "bin\\" + strDll + ".dll").GetType(strType);
                        instance.dicConstructor.Add(strType + strDll, tp.GetConstructor(new Type[] { }));
                    }
                }
                lock (instance.dicMethod)
                {
                    if (!instance.dicMethod.ContainsKey(strType + strDll + strMethod))
                    {
                        Type tp = Assembly.LoadFile(System.AppDomain.CurrentDomain.BaseDirectory
                          + "bin\\" + strDll + ".dll").GetType(strType);
                        instance.dicMethod.Add(strType + strDll + strMethod, tp.GetMethod(strMethod));
                    };
                }
                FacadeBase facade = instance.dicConstructor[strType + strDll].Invoke(null) as FacadeBase;
                facade.Context = context;
                strReturn = instance.dicMethod[strType + strDll + strMethod]
                    .Invoke(facade, context.Request["invokeParam"].Split('|')).ToString();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                strReturn = new
                {
                    errcode = "-1",
                    errmsg = "操作出现异常，请联系管理员"
                }.ToJsonString();
            }
            context.Response.Write(strReturn);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class InvokeCache
    {
        public Dictionary<string, ConstructorInfo> dicConstructor;
        public Dictionary<string, MethodInfo> dicMethod;
    }
}
