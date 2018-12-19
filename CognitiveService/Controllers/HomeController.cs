using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CognitiveService.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.ProjectOxford.Face;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CognitiveService.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFaceServiceClient _faceserviceclient;
        IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment, IConfiguration config)
        {
            _faceserviceclient = new FaceServiceClient(config.GetValue<string>("APIKey"));
            _hostingEnvironment = hostingEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(List<IFormFile> file)
        {
            var model = new VeryfiModel();
            model.imgList = await saveIMGAsync(file);
            try
            {
                Guid faceid1;
                Guid faceid2;
                // Detect the face in each image - need the FaceId for each
                using (Stream faceimagestream = file[0].OpenReadStream())
                {
                    var faces = await _faceserviceclient.DetectAsync(faceimagestream, returnFaceId: true);
                    if (faces.Length > 0)
                        faceid1 = faces[0].FaceId;
                    else
                        throw new Exception("No face found in image 1.");
                }
                using (Stream faceimagestream = file[1].OpenReadStream())
                {
                    var faces = await _faceserviceclient.DetectAsync(faceimagestream, returnFaceId: true);
                    if (faces.Length > 0)
                        faceid2 = faces[0].FaceId;
                    else
                        throw new Exception("No face found in image 2.");
                }

                // Verify the faces
                var mm  = await _faceserviceclient.VerifyAsync(faceid1, faceid2);
                model.Confidence = mm.Confidence;
                model.IsIdentical = mm.IsIdentical;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return View(model);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<List<string>> saveIMGAsync(List<IFormFile> files)
        {
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath);
            var list = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(uploads, file.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        list.Add(file.FileName);
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            return list;

        }
    }
}
