using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ChunkFileUpload.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }


        private string HashChunk(byte[] data)
        {
            //   var ss = Encoding.UTF8.GetString(data);
            //  var s = String.Empty;
            var hash = MD5.Create().ComputeHash(data);
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString().ToLower().Replace("-", "");
        }
        [HttpPost]

        public void UploadFiles()
        {


            string _uploadFolder = "~/upload/";

            var queryString = Request.Form;

            if (queryString.Count == 0) return;

            // Read parameters

            var uploadToken = queryString.Get("upload_Token");

            int resumableChunkNumber = int.Parse(queryString.Get("resumableChunkNumber"));

            var resumableFilename = queryString.Get("resumableFilename");

            var resumableIdentifier = queryString.Get("resumableIdentifier");

            int resumableChunkSize = int.Parse(queryString.Get("resumableChunkSize"));

            long resumableTotalSize = long.Parse(queryString.Get("resumableTotalSize"));

            var filePath = string.Format("{0}/{1}/{1}.part{2}", _uploadFolder, uploadToken, resumableChunkNumber.ToString("0000"));
            var chunkData = new byte[resumableChunkSize];
            var localFilePath = Server.MapPath(filePath);
            var hash = queryString.Get("checksum");
            Request.Files[0].InputStream.Read(chunkData, 0, Convert.ToInt32(resumableChunkSize));
            var HashFound = HashChunk(chunkData);
            var directory = System.IO.Path.GetDirectoryName(localFilePath);

            if (!System.IO.Directory.Exists(localFilePath))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            if (!hash.Equals(HashFound))
            {

            }
            else
            {
                if (Request.Files.Count == 1)
                {
                    // save chunk
                    if (!System.IO.File.Exists(localFilePath))
                    {

                        Request.Files[0].SaveAs(localFilePath);
                    }

                    // Check if all chunks are ready and save file
                    var files = System.IO.Directory.GetFiles(directory);

                    if ((files.Length + 1) * (long)resumableChunkSize >= resumableTotalSize)
                    {
                        filePath = string.Format("{0}/{1}{2}", _uploadFolder, uploadToken, resumableFilename);

                        localFilePath = Server.MapPath(filePath);

                        using (var fs = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            foreach (string file in files.OrderBy(x => x))
                            {
                                var buffer = System.IO.File.ReadAllBytes(file);

                                fs.Write(buffer, 0, buffer.Length);

                                System.IO.File.Delete(file);
                            }
                            fs.Close();
                        }

                    }
                }

                else
                {
                    // log error

                }

            }
        }
        private byte[] ReadData(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }  
    }
}
 