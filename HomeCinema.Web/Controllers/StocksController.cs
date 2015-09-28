using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using HomeCinema.Data.Infrastructure;
using HomeCinema.Data.Repositories;
using HomeCinema.Entities;
using HomeCinema.Web.Infrastructure.Core;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/stocks")]
    public class StocksController : ApiControllerBase
    {
        private readonly IEntityBaseRepository<Stock> _stocksRepository;

        public StocksController(IEntityBaseRepository<Stock> stocksRepository, 
            IEntityBaseRepository<Error> errorsRepository, IUnitOfWork unitOfWork) : base(errorsRepository, unitOfWork)
        {
            _stocksRepository = stocksRepository;
        }

        [Route("movie/{id:int}")]
        public HttpResponseMessage Get(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                IEnumerable<Stock> stocks = _stocksRepository.FindBy(s => s.IsAvailable);
                IEnumerable<StockViewModel> stocksVM = Mapper.Map<IEnumerable<Stock>, IEnumerable<StockViewModel>>(stocks);
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, stocksVM);

                return response;
            });
        }
    }
}
