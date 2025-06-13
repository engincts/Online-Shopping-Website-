using System;
using System.Collections.Generic;

namespace E_ticaret_Sitesi.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
