using E_ticaret_Sitesi.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace E_ticaret_Sitesi.ViewModels
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new();
        public List<SelectListItem> AddressList { get; set; } = new();

        [Required(ErrorMessage = "Lütfen bir adres seçin.")]
        public int? SelectedAddressId { get; set; }
    }


}