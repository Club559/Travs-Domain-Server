using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.picture
{
    internal class delete : RequestHandler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            if (query.AllKeys.Length == 0)
            {
                string queryString = string.Empty;
                string currUrl = context.Request.RawUrl;
                int iqs = currUrl.IndexOf('?');
                if (iqs >= 0)
                {
                    query =
                        HttpUtility.ParseQueryString((iqs < currUrl.Length - 1)
                            ? currUrl.Substring(iqs + 1)
                            : String.Empty);
                }
            }

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                var cmd = db.CreateQuery();

                string user = query["guid"];

                string owner = "";
                bool isOwner = false;

                cmd.CommandText = "SELECT guid FROM sprites WHERE id=@id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", query["id"]);

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (!rdr.HasRows) return;
                    rdr.Read();
                    
                    owner = rdr.GetString("guid");

                    if (user == owner)
                    {
                        isOwner = true;
                    }
                }
                byte[] status = Encoding.UTF8.GetBytes("<Error>You can't delete this sprite</Error>");
                if (isOwner)
                {
                    cmd = db.CreateQuery();
                    cmd.CommandText = "DELETE FROM sprites WHERE(id=@id AND guid=@guid) LIMIT 1";
                    cmd.Parameters.AddWithValue("@id", query["id"]);
                    cmd.Parameters.AddWithValue("@guid", owner);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        status = Encoding.UTF8.GetBytes("<Success/>");
                    }
                    context.Response.OutputStream.Write(status, 0, status.Length);
                    return;
                }

                context.Response.OutputStream.Write(status, 0, status.Length);
            }
        }
    }
}