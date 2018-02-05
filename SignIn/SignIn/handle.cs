using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using DataAccess; 
using WeChatFramework;
namespace SignIn
{
    public class handle : WebFramework.ProcessBase
    {
        public handle()
        {
        }

        public string GetOpenIdByCode(string code)
        {
            Dictionary<string, object> dicRst = new Dictionary<string, object>();
            try
            {
                API api = new API();
                //dicRst.Add("openId", api.GetOpenIdByCode(code));
                dicRst.Add("openId", code);
            }
            catch (Exception ex)
            {
                WeChatFramework.ErrorLog.WriteLog(ex);
                dicRst.Add("errmsg", "出现错误，请联系管理员");
            }
            return dicRst.ToJsonString();
        }

        public string GetInfo(string openId)
        {
            Dictionary<string, object> dicRst = new Dictionary<string, object>();
            IDataAccess data = null;
            try
            {
                API api = new API();
                data = DataAccessFactory.Instance.CreateDataAccess();
                data.Open();
                CmdParameterCollection paras = new CmdParameterCollection();
                paras.Add("@openId", openId);
                IDataReader reader = data.ExecuteReader(@"
select * from SignRecord 
where OpenId = @openId", paras);
                if (reader.Read())
                {
                    dicRst.Add("count", 1);
                    dicRst.Add("type", reader["UserType"]);
                    dicRst.Add("code", reader["Code"]);
                    dicRst.Add("name", reader["UserName"]);
                    dicRst.Add("phone", reader["PhoneNumber"]);
                }
                else
                    dicRst.Add("count", 0);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                dicRst.Add("errmsg", "出现错误，请联系管理员");
            }
            finally
            {
                data.Close();
            }
            return dicRst.ToJsonString();
        }

        public string SignIn(string type, string openId, string code, string name, string phoneNumber)
        {
            Dictionary<string, object> dicRst = new Dictionary<string, object>();
            IDataAccess data = null;
            try
            {
                data = DataAccessFactory.Instance.CreateDataAccess();
                data.Open();
                if (type == "Staff")
                {
                    CmdParameterCollection paras = new CmdParameterCollection();
                    paras.Add("@WorkNumber", code);
                    paras.Add("@UserName", name);
                    object obj = data.ExecuteScalar(@" 
select count(1) from Staff 
where WorkNumber=@WorkNumber and UserName=@UserName ", paras);
                    if (obj == null || obj == DBNull.Value)
                    {
                        dicRst.Add("errmsg", "工号与姓名不符，请核对");
                    }
                    else
                    {
                        if (Convert.ToInt32(obj) == 1)
                        {
                            CmdParameterCollection paras3 = new CmdParameterCollection();
                            paras3.Add("@Code", code);
                            IDataReader reader = data.ExecuteReader(@"
select * from SignRecord
where Code=@Code", paras3);
                            if (reader.Read())
                            {
                                dicRst.Add("result", "您已经于" +
                                    Convert.ToDateTime(reader["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss") +
                                    "进行过签到，无需再次签到");
                                reader.Close();
                            }
                            else
                            {
                                reader.Close();
                                CmdParameterCollection paras2 = new CmdParameterCollection();
                                paras2.Add("@Id", BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0));
                                paras2.Add("@OpenId", openId);
                                paras2.Add("@UserType", type);
                                paras2.Add("@Code", code);
                                paras2.Add("@UserName", name);
                                paras2.Add("@PhoneNumber", phoneNumber);
                                paras2.Add("@CreateTime", DateTime.Now);
                                data.ExecuteNonQuery(@"
insert into SignRecord(Id,OpenId,UserType,Code,UserName,PhoneNumber,CreateTime)
values(@Id,@OpenId,@UserType,@Code,@UserName,@PhoneNumber,@CreateTime)", paras2);
                                dicRst.Add("result", "签到成功！");
                            }
                        }
                        else
                        {
                            dicRst.Add("errmsg", "工号与姓名不符，请核对");
                        }
                    }
                }
                else
                {
                    dicRst.Add("errmsg", "暂未开通服务");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                dicRst.Add("errmsg", "签到错误，请联系管理员");
            }
            finally
            {
                data.Close();
            }
            return dicRst.ToJsonString();
        }

        public string GetSignInUserList()
        {
            IDataAccess data = null;
            try
            {
                data = DataAccessFactory.Instance.CreateDataAccess();
                data.Open();
                DataTable dt = data.ExecuteDataSet(@"
SELECT row_number() over(order by a.Code) as 序号, a.Code as 工号,a.UserName as 姓名 FROM SignRecord a
where a.UserType = 'Staff'").Tables[0];
                MemoryStream ms = new MemoryStream();
                MemoryStream ms2 = new MemoryStream();
                dt.WriteXml(ms);
                dt.WriteXmlSchema(ms2);
                return new
                   {
                       Data = Encoding.Default.GetString(ms.ToArray()),
                       Schema = Encoding.Default.GetString(ms2.ToArray())
                   }.ToJsonString();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                return new
                {
                    errcode = "-1",
                    errmsg = ex.Message
                }.ToJsonString();
            }
            finally
            {
                data.Close();
            }
        }

    }
}