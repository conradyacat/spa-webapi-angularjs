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
using HomeCinema.Web.Infrastructure.Extensions;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/customers")]
    public class CustomersController : ApiControllerBase
    {
        private readonly IEntityBaseRepository<Customer> _customersRepository;

        public CustomersController(IEntityBaseRepository<Customer> customersRepository,
            IEntityBaseRepository<Error> errorsRepository, IUnitOfWork unitOfWork) : base(errorsRepository, unitOfWork)
        {
            _customersRepository = customersRepository;
        }

        public HttpResponseMessage Get(HttpRequestMessage request, string filter)
        {
            filter = filter.ToLower().Trim();

            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                var customers = _customersRepository.GetAll()
                    .Where(c => c.Email.ToLower().Contains(filter) ||
                    c.FirstName.ToLower().Contains(filter) ||
                    c.LastName.ToLower().Contains(filter)).ToList();

                var customersVm = Mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerViewModel>>(customers);

                response = request.CreateResponse<IEnumerable<CustomerViewModel>>(HttpStatusCode.OK, customersVm);

                return response;
            });
        }

        [Route("details/{id:int}")]
        public HttpResponseMessage Get(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                var customer = _customersRepository.GetSingle(id);

                CustomerViewModel customerVm = Mapper.Map<Customer, CustomerViewModel>(customer);

                response = request.CreateResponse<CustomerViewModel>(HttpStatusCode.OK, customerVm);

                return response;
            });
        }

        [HttpGet]
        [Route("search/{page:int=0}/{pageSize=4}/{filter?}")]
        public HttpResponseMessage Search(HttpRequestMessage request, int? page, int? pageSize, string filter = null)
        {
            int currentPage = page.Value;
            int currentPageSize = pageSize.Value;

            return CreateHttpResponse(request, () =>
            {
                List<Customer> customers = null;

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().ToLower();

                    customers = _customersRepository.GetAll()
                        .OrderBy(c => c.ID)
                        .Where(c => c.LastName.ToLower().Contains(filter) ||
                            c.IdentityCard.ToLower().Contains(filter) ||
                            c.FirstName.ToLower().Contains(filter))
                        .ToList();
                }
                else
                {
                    customers = _customersRepository.GetAll().ToList();
                }

                int totalMovies = customers.Count();
                customers = customers.Skip(currentPage * currentPageSize).Take(currentPageSize).ToList();

                IEnumerable<CustomerViewModel> customersVM = Mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerViewModel>>(customers);

                var pagedSet = new PaginationSet<CustomerViewModel>
                {
                    Page = currentPage,
                    TotalCount = totalMovies,
                    TotalPages = (int)Math.Ceiling((decimal)totalMovies / currentPageSize),
                    Items = customersVM
                };

                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, pagedSet);
                return response;
            });
        }

        [HttpPost]
        [Route("update")]
        public HttpResponseMessage Update(HttpRequestMessage request, CustomerViewModel customerVM)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                if (!ModelState.IsValid)
                {
                    response = request.CreateResponse(HttpStatusCode.BadRequest,
                        ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                            .Select(m => m.ErrorMessage).ToArray());
                }
                else
                {
                    Customer customer = _customersRepository.GetSingle(customerVM.ID);
                    customer.UpdateCustomer(customerVM);

                    _unitOfWork.Commit();

                    response = request.CreateResponse(HttpStatusCode.OK);
                }

                return response;
            });
        }

        [HttpPost]
        public HttpResponseMessage Register(HttpRequestMessage request, CustomerViewModel customerVM)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;

                if (!ModelState.IsValid)
                {
                    response = request.CreateResponse(HttpStatusCode.BadGateway,
                        ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                            .Select(m => m.ErrorMessage).ToArray());
                }
                else
                {
                    if (_customersRepository.FindBy(c => c.Email == customerVM.Email && c.IdentityCard == customerVM.IdentityCard).FirstOrDefault() != null)
                    {
                        ModelState.AddModelError("Invalid user", "Email or Identity Card number already exists");
                        response = request.CreateResponse(HttpStatusCode.BadGateway,
                            ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                .Select(m => m.ErrorMessage).ToArray());
                    }
                    else
                    {
                        var customer = new Customer();
                        customer.UpdateCustomer(customerVM);
                        _customersRepository.Add(customer);

                        _unitOfWork.Commit();

                        customerVM = Mapper.Map<Customer, CustomerViewModel>(customer);
                        response = request.CreateResponse(HttpStatusCode.Created, customerVM);
                    }
                }

                return response;
            });
        }
    }
}
