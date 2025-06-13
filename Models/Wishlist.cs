﻿using System;
using System.Collections.Generic;

namespace E_ticaret_Sitesi.Models;

public partial class Wishlist
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
