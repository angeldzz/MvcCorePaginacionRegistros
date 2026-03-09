using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;

namespace MvcCorePaginacionRegistros.Repositories
{
    #region VISTAS Y PROCEDIMIENTOS ALMACENADOS
    /*
     --ROW_NUMER() OVER (ORDER BY COLUMNA)
--SELECT TO SELECT
SELECT * FROM
(SELECT ROW_NUMBER() OVER (ORDER BY DEPT_NO) AS POSICION,
DEPT_NO, DNOMBRE, LOC FROM DEPT) QUERY
WHERE QUERY.POSICION = 2

create VIEW V_DEPARTAMENTO_INDIVIDUAL
AS
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY DEPT_NO) as int) AS POSICION,
	DEPT_NO, DNOMBRE, LOC FROM DEPT
GO

SELECT * FROM V_DEPARTAMENTO_INDIVIDUAL
WHERE POSICION = 1
-----------------------------------------
LO MISMO CON UN PROCEDIMIENTO ALMACENADO
    
CREATE PROCEDURE SP_GRUPO_DEPARTAMENTOS
(@posicion int)
AS
	SELECT DEPT_NO, DNOMBRE, LOC FROM V_DEPARTAMENTO_INDIVIDUAL
	WHERE POSICION >= @posicion AND POSICION < (@posicion + 2)
GO

EXEC SP_GRUPO_DEPARTAMENTOS 1
    ------------------------------------------
Paginacion de Empleados

CREATE VIEW V_GRUPO_EMPLEADOS
AS
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY EMP_NO) AS INT)
	AS POSICION,EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO FROM EMP
GO
SELECT * FROM V_GRUPO_EMPLEADOS

CREATE PROCEDURE SP_GRUPO_EMPLEADOS (@posicion int)
AS
	SELECT EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO 
	FROM V_GRUPO_EMPLEADOS
	WHERE POSICION >= @posicion AND POSICION < (@posicion + 3)
GO

	SELECT EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO 
	FROM V_GRUPO_EMPLEADOS
	WHERE POSICION >= 2 AND POSICION < (2 + 3)

EXEC SP_GRUPO_EMPLEADOS 1
EXEC SP_GRUPO_EMPLEADOS 4
    ---------------------------------------------
    PARA FILTRAR NO PODEMOS USAR LA VISTA
    DEBEMOS USAR EL PROCEDIMIENTO ALMACENADO CON PARAMETROS
    ---------------------------------------------

CREATE PROCEDURE SP_GRUPO_EMPLEADOS_OFICIO
(@posicion int , @oficio nvarchar(50), @registros int out)
AS
	--almacenamos el numero de registros filtrados
	SELECT @registros = COUNT(EMP_NO) from EMP WHERE OFICIO = @oficio
	SELECT EMP_NO,APELLIDO, OFICIO, SALARIO, DEPT_NO FROM
	(SELECT CAST(ROW_NUMBER() OVER (ORDER BY EMP_NO) AS INT) AS POSICION,
	EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO 
	FROM V_GRUPO_EMPLEADOS
	WHERE OFICIO=@oficio) QUERY
	WHERE QUERY.POSICION >= @posicion AND QUERY.POSICION < (@posicion + 3)
GO

EXEC SP_GRUPO_EMPLEADOS_OFICIO 1, 'ANALISTA'

----------------------------------------------
EMPLEADOS POR DEPARTAMENTOS 

CREATE VIEW V_GRUPO_EMPLEADOS
AS
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY EMP_NO) AS INT)
	AS POSICION,EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO FROM EMP
GO
SELECT * FROM V_GRUPO_EMPLEADOS

CREATE PROCEDURE SP_GRUPO_EMPLEADOS_DEPARTAMENTO
(@posicion int , @iddept int, @registros int out)
AS
	--almacenamos el numero de registros filtrados
	SELECT @registros = COUNT(EMP_NO) from EMP WHERE DEPT_NO = @iddept
	SELECT EMP_NO,APELLIDO, OFICIO, SALARIO, DEPT_NO FROM
	(SELECT CAST(ROW_NUMBER() OVER (ORDER BY EMP_NO) AS INT) AS POSICION,
	EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO 
	FROM V_GRUPO_EMPLEADOS
	WHERE DEPT_NO=@iddept) QUERY
	WHERE QUERY.POSICION = @posicion
GO

DECLARE @total_filas INT;

EXEC SP_GRUPO_EMPLEADOS_DEPARTAMENTO 
    @posicion = 1, 
    @iddept = 10, 
    @registros = @total_filas OUTPUT;
    */
    #endregion
    public class RepositoryHospital
    {
        private HospitalContext context;
        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }
        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await context.VistaDepartamentos.CountAsync();
        }
        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await context.Departamentos.ToListAsync();
        }
        public async Task<Departamento> FindDepartamentoAsync(int iddept)
        {
            return await context.Departamentos.Where(d => d.IdDepartamento == iddept).FirstOrDefaultAsync();
        }
        public async Task<int> GetEmpleadosDepartamentosCountAsync(int iddept)
        {
            return await this.context.Empleados.Where(e => e.IdDepartamento == iddept).CountAsync();
        }
        public async Task<ModelEmpleadosOficio> GetGrupoEmpleadosDepartamentosOutAsync(int posicion, int iddept)
        {
            string sql = "SP_GRUPO_EMPLEADOS_DEPARTAMENTO @posicion, @iddept, @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamDept = new SqlParameter("@iddept", iddept);
            SqlParameter pamRegistros = new SqlParameter("@registros", 0);
            pamRegistros.DbType = System.Data.DbType.Int32;
            pamRegistros.Direction = System.Data.ParameterDirection.Output;
            var consulta =
                this.context.Empleados.FromSqlRaw(sql, pamPosicion, pamDept, pamRegistros);
            //HASTA QUE NO HEMOS EXTRAIDO LOS DATOS(EMPLEADOS)
            //NO SE LIBERAN LOS PARAMETROS DE SALIDA
            List<Empleado> empleados = await consulta.ToListAsync();
            int numeroRegistros = (int)pamRegistros.Value;
            return new ModelEmpleadosOficio
            {
                Empleados = empleados,
                NumeroRegistros = numeroRegistros
            };
        }
        public async Task<ModelEmpleadoDepartamento> GetGrupoEmpleadosDepartamentosEFAsync(int posicion, int iddept)
        {
            Empleado empleado = await this.context.Empleados
                .Where(e => e.IdDepartamento == iddept)
                .Skip(posicion - 1)
                .Take(1)
                .FirstOrDefaultAsync();
            int totalEmpleados = await this.context.Empleados
                .Where(e => e.IdDepartamento == iddept)
                .CountAsync();
            Departamento departamento = await this.context.Departamentos
                .FirstOrDefaultAsync(d => d.IdDepartamento == iddept);
            return new ModelEmpleadoDepartamento
            {
                Empleado = empleado,
                numRegistros = totalEmpleados,
                Departamento = departamento
            };
        }
        public async Task<VistaDepartamento> GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento departamento =
                 await this.context.VistaDepartamentos.Where(d => d.Posicion == posicion).FirstOrDefaultAsync();
            return departamento;
        }
        public async Task<List<VistaDepartamento>> GetGrupoVistaDepartamentosAsync(int posicion)
        {
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion && datos.Posicion < posicion + 2
                           select datos;
            return await consulta.ToListAsync();
        }
        public async Task<List<Departamento>> GetGrupoDepartamentosAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            return await this.context.Departamentos.FromSqlRaw(sql, pamPosicion).ToListAsync();
        }
        public async Task<int> GetEmpleadosCountAsync()
        {
            return await context.Empleados.CountAsync();
        }
        public async Task<List<Empleado>> GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            return await this.context.Empleados.FromSqlRaw(sql, pamPosicion).ToListAsync();
        }
        public async Task<int> GetEmpleadosOficioCountAsync(string oficio)
        {
            return await this.context.Empleados.Where(e => e.Oficio == oficio).CountAsync();
        }
        public async Task<List<Empleado>> GetGrupoEmpleadosOficioAsync(int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO @posicion, @oficio";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            return await this.context.Empleados.FromSqlRaw(sql, pamPosicion, pamOficio).ToListAsync();
        }
        public async Task<ModelEmpleadosOficio> GetGrupoEmpleadosOficioOutAsync(int posicion, string oficio)
        {
            string sql = "EXEC SP_GRUPO_EMPLEADOS_OFICIO @posicion, @oficio, @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamRegistros = new SqlParameter("@registros", 0);
            pamRegistros.DbType = System.Data.DbType.Int32;
            pamRegistros.Direction = System.Data.ParameterDirection.Output;
            var consulta = 
                this.context.Empleados.FromSqlRaw(sql, pamPosicion, pamOficio, pamRegistros);
            //HASTA QUE NO HEMOS EXTRAIDO LOS DATOS(EMPLEADOS)
            //NO SE LIBERAN LOS PARAMETROS DE SALIDA
            List<Empleado> empleados = await consulta.ToListAsync();
            int numeroRegistros = (int)pamRegistros.Value;
            return new ModelEmpleadosOficio
            {
                Empleados = empleados,
                NumeroRegistros = numeroRegistros
            };
        }
    }
}
