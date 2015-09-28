using System;
using HomeCinema.Entities;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Infrastructure.Extensions
{
    public static class EntitiesExtensions
    {
        public static void UpdateCustomer(this Customer customer, CustomerViewModel customerVM)
        {
            customer.FirstName = customerVM.FirstName;
            customer.LastName = customerVM.LastName;
            customer.IdentityCard = customerVM.IdentityCard;
            customer.Mobile = customerVM.Mobile;
            customer.DateOfBirth = customerVM.DateOfBirth;
            customer.Email = customerVM.Email;
            customer.UniqueKey = (customerVM.UniqueKey == Guid.Empty) ? Guid.NewGuid() : customerVM.UniqueKey;
            customer.RegistrationDate = customerVM.RegistrationDate == DateTime.MinValue ? DateTime.Now : customerVM.RegistrationDate;
        }

        public static void UpdateMovie(this Movie movie, MovieViewModel movieVM)
        {
            movie.Title = movieVM.Title;
            movie.Description = movieVM.Description;
            movie.GenreId = movieVM.GenreId;
            movie.Director = movieVM.Director;
            movie.Writer = movieVM.Writer;
            movie.Producer = movieVM.Producer;
            movie.Rating = movieVM.Rating;
            movie.TrailerURI = movieVM.TrailerURI;
            movie.ReleaseDate = movieVM.ReleaseDate;
        }
    }
}
