using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AutoMapper;
using HomeCinema.Data.Infrastructure;
using HomeCinema.Data.Repositories;
using HomeCinema.Entities;
using HomeCinema.Web.Infrastructure.Core;
using HomeCinema.Web.Infrastructure.Extensions;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/movies")]
    public class MoviesController : ApiControllerBase
    {
        private readonly IEntityBaseRepository<Movie> _moviesRepository;

        public MoviesController(IEntityBaseRepository<Movie> moviesRepository, IEntityBaseRepository<Error> errorsRepository,
            IUnitOfWork unitOfWork) : base(errorsRepository, unitOfWork)
        {
            _moviesRepository = moviesRepository;
        }

        [AllowAnonymous]
        [Route("latest")]
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                var movies = _moviesRepository.GetAll().OrderByDescending(m => m.ReleaseDate).Take(6).ToList();
                IEnumerable<MovieViewModel> moviesVM = Mapper.Map<IEnumerable<Movie>, IEnumerable<MovieViewModel>>(movies);
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, moviesVM);

                return response;
            });
        }

        [AllowAnonymous]
        [Route("{page:int=0}/{pageSize=3}/{filter?}")]
        public HttpResponseMessage Get(HttpRequestMessage request, int? page, int? pageSize, string filter = null)
        {
            int currentPage = page.Value;
            int currentPageSize = pageSize.Value;

            return CreateHttpResponse(request, () =>
            {
                List<Movie> movies = null;

                if (!string.IsNullOrEmpty(filter))
                {
                    movies = _moviesRepository.GetAll()
                        .OrderBy(m => m.ID)
                        .Where(m => m.Title.ToLower().Contains(filter.ToLower().Trim()))
                        .ToList();
                }
                else
                {
                    movies = _moviesRepository.GetAll().ToList();
                }

                int totalMovies = movies.Count();
                movies = movies.Skip(currentPage * currentPageSize).Take(currentPageSize).ToList();

                IEnumerable<MovieViewModel> moviesVM = Mapper.Map<IEnumerable<Movie>, IEnumerable<MovieViewModel>>(movies);

                var pagedSet = new PaginationSet<MovieViewModel>
                {
                    Page = currentPage,
                    TotalCount = totalMovies,
                    TotalPages = (int)Math.Ceiling((decimal)totalMovies / currentPageSize),
                    Items = moviesVM
                };

                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, pagedSet);

                return response;
            });
        }

        [HttpGet]
        [Route("details/{id:int}")]
        public HttpResponseMessage Get(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                var movie = _moviesRepository.GetSingle(id);
                var movieVM = Mapper.Map<Movie, MovieViewModel>(movie);
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, movieVM);
                return response;
            });
        }

        [MimeMultipart]
        [Route("images/upload")]
        public HttpResponseMessage Post(HttpRequestMessage request, int movieId)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                var movieOld = _moviesRepository.GetSingle(movieId);

                if (movieOld == null)
                {
                    response = request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid movie");
                }
                else
                {
                    var uploadPath = HttpContext.Current.Server.MapPath("~/Content/images/movies");
                    var multipartFormDataStreamProvider = new UploadMultipartFormProvider(uploadPath);
                    Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);
                    string localFileName = multipartFormDataStreamProvider.FileData.Select(d => d.LocalFileName).FirstOrDefault();

                    var fileUploadResult = new FileUploadResult
                    {
                        LocalFilePath = localFileName,
                        FileName = Path.GetFileName(localFileName),
                        FileLength = new FileInfo(localFileName).Length
                    };

                    movieOld.Image = fileUploadResult.FileName;
                    _moviesRepository.Edit(movieOld);
                    _unitOfWork.Commit();

                    response = request.CreateResponse(HttpStatusCode.OK, fileUploadResult);
                }

                return response;
            });
        }

        [HttpPost]
        [Route("update")]
        public HttpResponseMessage Update(HttpRequestMessage request, MovieViewModel movieVM)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                if (!ModelState.IsValid)
                {
                    response = request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    var movie = _moviesRepository.GetSingle(movieVM.ID);
                    if (movie == null)
                    {
                        response = request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid movie");
                    }
                    else
                    {
                        movie.UpdateMovie(movieVM);
                        movieVM.Image = movie.Image;
                        _moviesRepository.Edit(movie);
                        _unitOfWork.Commit();

                        response = request.CreateResponse(HttpStatusCode.OK, movieVM);
                    }
                }

                return response;
            });
        }

        [HttpPost]
        [Route("add")]
        public HttpResponseMessage Add(HttpRequestMessage request, MovieViewModel movieVM)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                if (!ModelState.IsValid)
                {
                    response = request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    var movie = new Movie();
                    movie.UpdateMovie(movieVM);

                    for (int i = 0; i < movieVM.NumberOfStocks; i++)
                    {
                        var stock = new Stock
                        {
                            IsAvailable = true,
                            Movie = movie,
                            UniqueKey = Guid.NewGuid()
                        };
                        movie.Stocks.Add(stock);
                    }

                    _moviesRepository.Add(movie);
                    _unitOfWork.Commit();

                    movieVM = Mapper.Map<Movie, MovieViewModel>(movie);
                    response = request.CreateResponse(HttpStatusCode.Created, movieVM);
                }

                return response;
            });
        }
    }
}
