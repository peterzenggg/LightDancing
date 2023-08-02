
using System;
using System.IO;
using System.Net;

namespace AutoPackage
{
    public class FTPPackage
    {
        public string Server { get; set; }
        public FileInfo File { get; set; }
        public ICredentials Credentials { get; set; }

		public void Upload()
		{
            Console.WriteLine("Start upload package");
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Server + "/" + File.Name);
			request.Credentials = Credentials;
			request.KeepAlive = false;
			request.UsePassive = true;
			request.Method = "STOR";
			request.UseBinary = true;
			request.ContentLength = File.Length;
			byte[] bytes = new byte[2048];

            using FileStream fs = File.OpenRead();
            using Stream stream = request.GetRequestStream();

            for (int contentLen = fs.Read(bytes, 0, 2048); contentLen != 0; contentLen = fs.Read(bytes, 0, 2048))
            {
                stream.Write(bytes, 0, contentLen);
            }

            Console.WriteLine("Upload package completed");
        }
	}
}
