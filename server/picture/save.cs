using db;
using db.utils;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace server.picture
{
    internal class save : RequestHandler
    {
        private byte[] buff = new byte[0x10000];

        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            //using (var rdr = new StreamReader(context.Request.InputStream))
            //    query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            HttpMultipartParser parser = new HttpMultipartParser(context.Request.InputStream, "data");

            byte[] status;
            //if (query.AllKeys.Length == 0)
            //{
            //    string queryString = string.Empty;
            //    string currUrl = context.Request.RawUrl;
            //    int iqs = currUrl.IndexOf('?');
            //    if (iqs >= 0)
            //    {
            //        query =
            //            HttpUtility.ParseQueryString((iqs < currUrl.Length - 1)
            //                ? currUrl.Substring(iqs + 1)
            //                : String.Empty);
            //    }
            //}

            
            if (parser.Success)
            {
                using (var db = new Database(Program.Settings.GetValue("conn")))
                {
                    Account acc = db.Verify(parser.Parameters["guid"], parser.Parameters["password"]);
                    var cmd = db.CreateQuery();
                    var guid = parser.Parameters["guid"];
                    if (guid == "Admin" && !acc.Admin)
                        guid = "Guest";
                    if (parser.Parameters["admin"] == "true" && acc.Admin)
                        guid = "Admin";

                    cmd.CommandText = "INSERT INTO sprites(guid, name, dataType, tags, data, fileSize) VALUES(@guid, @name, @dataType, @tags, @data, @fileSize)";
                    cmd.Parameters.AddWithValue("@guid", guid);
                    cmd.Parameters.AddWithValue("@name", parser.Parameters["name"]);
                    cmd.Parameters.AddWithValue("@dataType", parser.Parameters["datatype"]);
                    cmd.Parameters.AddWithValue("@tags", parser.Parameters["tags"].Replace(", ", ",").Replace(" ,", ",").Trim());
                    cmd.Parameters.AddWithValue("@data", parser.FileContents);
                    cmd.Parameters.AddWithValue("@fileSize", parser.FileContents.Length);

                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            status = Encoding.UTF8.GetBytes("<Success/>");
                        }
                        else
                        {
                            status = Encoding.UTF8.GetBytes("<Error>Account credentials not valid</Error>");
                        }
                        context.Response.OutputStream.Write(status, 0, status.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}