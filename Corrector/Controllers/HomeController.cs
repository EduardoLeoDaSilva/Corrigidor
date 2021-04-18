using Corrector.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Corrector.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("EnviarRespostas");
        }

        public IActionResult EnviarRespostas()
        {
            return View();
        }

        public IActionResult GabaritoQuestoes()
        {
            var gabaritoQuestaoJson = HttpContext.Session.GetString("gabaritoQuestao");
            var gabaritoObject = JsonConvert.DeserializeObject<GabaritoQuestao>(gabaritoQuestaoJson);
            HttpContext.Session.Clear();
            return View(gabaritoObject);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarRespostasCarregarGabarito([FromForm]IFormFile gabarito,  string questaos, GabaritoQuestao questao)
        {

            List<Questao> questoes = JsonConvert.DeserializeObject<List<Questao>>(questaos);
            questoes = questoes.OrderBy(x => x.NumeroQuetao).ToList();
            if (gabarito == null || gabarito.Length <= 0)
                return BadRequest("Arquivo Vazio");

            if (!Path.GetExtension(gabarito.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Somente extensão de tipo .xlsx");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            await using var stream = new MemoryStream();
            await gabarito.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            var gabaritoQuestao = new GabaritoQuestao();
            for (var row = 2; row <= rowCount; row++)
            {
                try
                {


                    questoes[row-2].Disciplina = worksheet.Cells[row, 1].Value?.ToString().Trim();
                    questoes[row-2].Gabarito = worksheet.Cells[row, 3].Value?.ToString().Trim();
                    if (questoes[row-2].Gabarito.ToLower() == questoes[row - 2].Resposta.ToLower())
                    {
                        questoes[row-2].Ponto = 1;
                        gabaritoQuestao.Total += 1;
                        
                    }
                    else if (questoes[row-2].Gabarito.ToLower() != questoes[row - 2].Resposta.ToLower() && questoes[row - 2].Resposta.ToLower() != "SR".ToLower() && questoes[row - 2].Resposta.ToLower() != "S".ToLower())
                    {
                        questoes[row-2].Ponto = -1;
                        gabaritoQuestao.Total -= 1;

                    }
                    else if(String.IsNullOrEmpty(questoes[row - 2].Resposta) || questoes[row - 2].Resposta.ToLower() == "SR".ToLower() || questoes[row - 2].Resposta.ToLower() == "S".ToLower())
                    {
                        questoes[row-2].Ponto = 0;
                        gabaritoQuestao.Total += 0;

                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }

            gabaritoQuestao.Questoes = questoes;


            HttpContext.Session.SetString("gabaritoQuestao", JsonConvert.SerializeObject(gabaritoQuestao));
            ViewBag.ListaQuestoes = gabaritoQuestao;
            return View("GabaritoQuestoes", gabaritoQuestao);


        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
