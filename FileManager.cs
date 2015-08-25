using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ChunkFileUpload
{
    public class FileManager
    {
        private string _UploadDirectory = HttpContext.Current.Server.MapPath("~/upload/");
        public string UploadDirectory
        {
            get { return this._UploadDirectory; }
            set { _UploadDirectory = value; }
        }

        public bool ChunkExists(string identifier, int chunkNumber, long chunkSize, string fileName)
        {
            string chunkFile = Path.Combine(UploadDirectory, identifier.ToString(), chunkNumber.ToString(), fileName);

            if(File.Exists(chunkFile))
            {
                FileInfo fileInfo = new FileInfo(chunkFile);
                long size = fileInfo.Length;
                return size == chunkSize;
            }
            return false;
        }

        public void StoreChunk(string identifier, int chunkNumber, Stream inputStream, string fileName, string hash)
        {
            string path = Path.Combine(UploadDirectory, identifier.ToString(), chunkNumber.ToString());
            string chunkFile = Path.Combine(UploadDirectory, identifier.ToString(), chunkNumber.ToString(), fileName);
            string hashFile = Path.Combine(UploadDirectory, identifier.ToString(), chunkNumber.ToString(), hash + ".txt");

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    //WriteErrorResponse(Resources.UploadDirectoryDoesnTExistAndCouldnTCreate);
                    //return false;
                }
            }

            Stream stream = null;
            Stream hashFileStream = null;
            try
            {
                stream = new FileStream(chunkFile, FileMode.Create);
                inputStream.CopyTo(stream, 16384);                

                //Creating <hash number>.txt file just to prove hashing is working
                //using file is to release the lock after hash file is created
                hashFileStream = File.Create(hashFile);
            }
            catch
            {
                //WriteErrorResponse(Resources.UnableToWriteOutFile);
                //return false;
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
                if (hashFileStream != null)
                    hashFileStream.Close();
            }
        }

        public bool AllChunksUploaded(string identifier, long chunkSize, long totalSize, string fileName)
        {
            long noOfChunks = totalSize / chunkSize;

            for (int chunkNo = 1; chunkNo <= noOfChunks; chunkNo++)
            {
                string chunkFile = Path.Combine(UploadDirectory, identifier.ToString(), chunkNo.ToString(), fileName);
                if(!File.Exists(chunkFile))
                {
                    return false;
                }
            }
            return true;
        }

        public void MergeAllChunks(string identifier, long chunkSize, long totalSize, string fileName)
        {
            long noOfChunks = totalSize / chunkSize;
            string newFilePath = Path.Combine(UploadDirectory, fileName);            

            Stream stream = null;
            try
            {
                stream = new FileStream(newFilePath, FileMode.Create);

                for (int chunkNo = 1; chunkNo <= noOfChunks; chunkNo++)
                {
                    string chunkFile = Path.Combine(UploadDirectory, identifier.ToString(), chunkNo.ToString(), fileName);

                    using (Stream fromStream = File.OpenRead(chunkFile))
                    {
                        fromStream.CopyTo(stream, 16384);
                    }
                }
            }
            catch
            {
                //WriteErrorResponse(Resources.UnableToWriteOutFile);
                //return false;
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }            
        }        
    }
}