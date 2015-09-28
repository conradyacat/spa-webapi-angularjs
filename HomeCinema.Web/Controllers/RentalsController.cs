using System;
using System.Collections.Generic;
using System.Linq;
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
    [RoutePrefix("api/rentals")]
    public class RentalsController : ApiControllerBase
    {
        private readonly IEntityBaseRepository<Rental> _rentalsRepository;
        private readonly IEntityBaseRepository<Customer> _customersRepository;
        private readonly IEntityBaseRepository<Stock> _stocksRepository;
        private readonly IEntityBaseRepository<Movie> _moviesRepository;

        public RentalsController(IEntityBaseRepository<Rental> rentalsRepository, IEntityBaseRepository<Customer> customersRepository,
            IEntityBaseRepository<Stock> stocksRepository, IEntityBaseRepository<Movie> moviesRepository,
            IEntityBaseRepository<Error> errorsRepository, IUnitOfWork unitOfWork) : base(errorsRepository, unitOfWork)
        {
            _rentalsRepository = rentalsRepository;
            _customersRepository = customersRepository;
            _stocksRepository = stocksRepository;
            _moviesRepository = moviesRepository;
        }

        [HttpGet]
        [Route("{id:int}/rentalhistory")]
        public HttpResponseMessage RentalHistory(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                List<RentalHistoryViewModel> rentalHistory = GetMovieRentalHistory(id);
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, rentalHistory);
                return response;
            });
        }

        [HttpPost]
        [Route("return/{rentalId:int}")]
        public HttpResponseMessage Return(HttpRequestMessage request, int rentalId)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                var rental = _rentalsRepository.GetSingle(rentalId);
                if (rental == null)
                {
                    response = request.CreateResponse(HttpStatusCode.NotFound, "Invalid rental");
                }
                else
                {
                    rental.Status = "Returned";
                    rental.Stock.IsAvailable = true;
                    rental.ReturnedDate = DateTime.Now;

                    _unitOfWork.Commit();

                    response = request.CreateResponse(HttpStatusCode.OK);
                }

                return response;
            });
        }

        [HttpPost]
        [Route("rent/{customerId:int}/{stockId:int}")]
        public HttpResponseMessage Rent(HttpRequestMessage request, int customerId, int stockId)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                var customer = _customersRepository.GetSingle(customerId);
                var stock = _stocksRepository.GetSingle(stockId);

                if (customer == null || stock == null)
                {
                    response = request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalidat Customer or Stock");
                }
                else
                {
                    if (stock.IsAvailable)
                    {
                        var rental = new Rental
                        {
                            CustomerId = customerId,
                            StockId = stockId,
                            RentalDate = DateTime.Now,
                            Status = "Borrowed"
                        };

                        _rentalsRepository.Add(rental);

                        stock.IsAvailable = false;

                        _unitOfWork.Commit();

                        RentalViewModel rentalVM = Mapper.Map<Rental, RentalViewModel>(rental);
                        response = request.CreateResponse(HttpStatusCode.Created, rentalVM);
                    }
                    else
                    {
                        response = request.CreateErrorResponse(HttpStatusCode.BadRequest, "Selected stock not available anymore");
                    }
                }

                return response;
            });
        }

        [HttpGet]
        [Route("rentalhistory")]
        public HttpResponseMessage TotalRentalHistory(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                var totalMoviesRentalHistory = new List<TotalRentalHistoryViewModel>();
                var movies = _moviesRepository.GetAll().ToList();

                foreach (var movie in movies)
                {
                    var totalRentalHistory = new TotalRentalHistoryViewModel
                    {
                        ID = movie.ID,
                        Title = movie.Title,
                        Image = movie.Image,
                        Rentals = GetMovieRentalHistoryPerDates(movie.ID)
                    };

                    if (totalRentalHistory.TotalRentals > 0)
                        totalMoviesRentalHistory.Add(totalRentalHistory);
                }

                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, totalMoviesRentalHistory);
                return response;
            });
        }

        private List<RentalHistoryViewModel> GetMovieRentalHistory(int movieId)
        {
            var rentalHistory = new List<RentalHistoryViewModel>();
            var rentals = new List<Rental>();
            Movie movie = _moviesRepository.GetSingle(movieId);

            foreach (var stock in movie.Stocks)
            {
                rentals.AddRange(stock.Rentals);
            }

            foreach (var rental in rentals)
            {
                Customer customer = _customersRepository.GetSingle(rental.CustomerId);
                var historyItem = new RentalHistoryViewModel
                {
                    ID = rental.ID,
                    StockId = rental.StockId,
                    RentalDate = rental.RentalDate,
                    ReturnedDate = rental.ReturnedDate.HasValue ? rental.ReturnedDate : null,
                    Status = rental.Status,
                    Customer = customer.FirstName + " " + customer.LastName
                };

                rentalHistory.Add(historyItem);
            }

            rentalHistory.Sort((r1, r2) => r2.RentalDate.CompareTo(r1.RentalDate));

            return rentalHistory;
        }

        private List<RentalHistoryPerDate> GetMovieRentalHistoryPerDates(int movieId)
        {
            var listHistory = new List<RentalHistoryPerDate>();
            var rentalHistory = GetMovieRentalHistory(movieId);

            if (rentalHistory.Count > 0)
            {
                var distinctDates = rentalHistory.Select(h => h.RentalDate.Date).Distinct().ToList();

                foreach (var distinctDate in distinctDates)
                {
                    var totalDateRentals = rentalHistory.Count(r => r.RentalDate.Date == distinctDate);
                    var movieRentalHistoryPerDate = new RentalHistoryPerDate
                    {
                        Date = distinctDate,
                        TotalRentals = totalDateRentals
                    };
                    listHistory.Add(movieRentalHistoryPerDate);
                }

                listHistory.Sort((r1, r2) => r1.Date.CompareTo(r2.Date));
            }

            return listHistory;
        }
    }
}
