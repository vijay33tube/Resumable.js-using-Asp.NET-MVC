using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChunkFileUpload.Controllers
{
    public class FileController : Controller
    {
        private FileManager fileManager = new FileManager();
        //
        // GET: /File/
        [HttpPost]
        public ActionResult Upload(string resumableIdentifier, int? resumableChunkNumber, long? resumableChunkSize, long? resumableTotalSize, string resumableChunkHash, string resumableFilename)
        {
            if (resumableIdentifier == null)
                return View();

            Stream stream = Request.Files[0].InputStream;

            fileManager.StoreChunk(resumableIdentifier, resumableChunkNumber.GetValueOrDefault(), stream, resumableFilename, resumableChunkHash);

            if (fileManager.AllChunksUploaded(resumableIdentifier, resumableChunkSize.GetValueOrDefault(), resumableTotalSize.GetValueOrDefault(), resumableFilename))
            {
                fileManager.MergeAllChunks(resumableIdentifier, resumableChunkSize.GetValueOrDefault(), resumableTotalSize.GetValueOrDefault(), resumableFilename);
            }

            Response.StatusCode = 200;

            return View();
        }

        [HttpGet]
        public ActionResult Upload(string resumableIdentifier, int? resumableChunkNumber, long? resumableChunkSize, string resumableFilename)
        {
            if (resumableIdentifier == null)
                return View();

            if(fileManager.ChunkExists(resumableIdentifier, resumableChunkNumber.GetValueOrDefault(), resumableChunkSize.GetValueOrDefault(), resumableFilename))
            {
                //do not upload chunk again
                Response.StatusCode = 200;
            }
            else
            {
                //chunk not on the server, upload it
                Response.StatusCode = 404;
            }

            return View();
        }        
    }
}
