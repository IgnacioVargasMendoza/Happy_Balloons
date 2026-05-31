using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface ICatalogoProductoRepositorio
    {
        List<CatalogoProductoDTO> ObtenerCatalogo(string busqueda = "", int? categoriaId = null, string orden = "");

        CatalogoProductoDTO ObtenerProductoCatalogoPorId(int productoId);

        List<CategoriaDTO> ObtenerCategorias();
    }
}
