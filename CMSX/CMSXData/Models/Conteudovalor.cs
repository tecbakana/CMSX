﻿using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class Conteudovalor
{
    public string Conteudoid { get; set; } = null!;

    public Guid? Unidadeid { get; set; }

    public decimal? Valor { get; set; }
}
