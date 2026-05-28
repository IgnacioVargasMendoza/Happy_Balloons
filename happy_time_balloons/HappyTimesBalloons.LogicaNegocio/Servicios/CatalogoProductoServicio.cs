using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class CatalogoProductoServicio : ICatalogoProductoServicio
    {
        private readonly ICatalogoProductoRepositorio _catalogoProductoRepositorio;

        public CatalogoProductoServicio(ICatalogoProductoRepositorio catalogoProductoRepositorio)
        {
            _catalogoProductoRepositorio = catalogoProductoRepositorio;
        }

        public List<CatalogoProductoDTO> ObtenerCatalogo()
        {
            return _catalogoProductoRepositorio.ObtenerCatalogo();
        }

        public CatalogoProductoDTO ObtenerProductoCatalogoPorId(int productoId)
        {
            return _catalogoProductoRepositorio.ObtenerProductoCatalogoPorId(productoId);
        }
    }
}
