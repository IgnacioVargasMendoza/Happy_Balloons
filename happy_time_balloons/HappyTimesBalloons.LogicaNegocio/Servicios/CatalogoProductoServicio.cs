using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class CatalogoProductoServicio : ICatalogoProductoServicio
    {
        private readonly ICatalogoProductoRepositorio _catalogoProductoRepositorio;

        public CatalogoProductoServicio(ICatalogoProductoRepositorio catalogoProductoRepositorio)
        {
            _catalogoProductoRepositorio = catalogoProductoRepositorio;
        }

        public List<CatalogoProductoDTO> ObtenerCatalogo(
            string busqueda = "",
            int? categoriaId = null,
            string orden = "")
        {
            return _catalogoProductoRepositorio.ObtenerCatalogo(
                busqueda,
                categoriaId,
                orden);
        }

        public CatalogoProductoDTO ObtenerProductoCatalogoPorId(int productoId)
        {
            return _catalogoProductoRepositorio.ObtenerProductoCatalogoPorId(productoId);
        }

        public List<CategoriaDTO> ObtenerCategorias()
        {
            return _catalogoProductoRepositorio.ObtenerCategorias();
        }
    }
}