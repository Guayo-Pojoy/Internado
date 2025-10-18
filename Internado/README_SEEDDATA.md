# Carga de Datos Ficticios - Sistema Internado

## Descripción

Este script (`SeedData.sql`) carga datos de prueba en la base de datos InternadoDB para facilitar las pruebas del sistema.

## Datos Incluidos

### 👥 Usuarios (7 cuentas)
- **1 Administrador**: Acceso total al sistema
- **3 Docentes**: Gestión académica
- **2 Médicos**: Gestión de salud
- **1 Dirección**: Gestión administrativa

### 📚 Datos Académicos
- **6 Cursos**: Matemáticas, Lenguaje, Ciencias, Historia, Inglés, Ed. Física
- **Asignaciones múltiples**: Docentes principales y colaboradores
- **Calificaciones**: Para 15 residentes en 3 cursos
- **Asistencias**: 10 días de registro para 2 cursos

### 🏥 Datos Médicos
- **10 Medicamentos**: Con stock y fechas de vencimiento
- **8 Consultas**: Últimos 15 días
- **Movimientos**: Entradas y salidas de inventario

### 🏠 Datos de Residentes
- **15 Residentes**: Distribuidos en 5 habitaciones
- **10 Habitaciones**: Con capacidades variadas
- **Datos completos**: Identificación, contactos, estado activo

## Credenciales de Acceso

| Usuario | Password | Rol |
|---------|----------|-----|
| `admin` | `Password123` | Administrador |
| `docente1` | `Password123` | Docente (María González) |
| `docente2` | `Password123` | Docente (Carlos Ramírez) |
| `docente3` | `Password123` | Docente (Ana Martínez) |
| `medico1` | `Password123` | Médico (Dr. Roberto Sánchez) |
| `medico2` | `Password123` | Médico (Dra. Patricia Torres) |
| `direccion` | `Password123` | Dirección (Lic. Fernando Díaz) |

## Asignaciones de Cursos

### Docente 1 (María González)
- **Principal en**: Matemáticas I, Ciencias Naturales
- **Colaborador en**: Historia de Honduras

### Docente 2 (Carlos Ramírez)
- **Principal en**: Lenguaje y Literatura, Inglés Básico
- **Colaborador en**: Matemáticas I

### Docente 3 (Ana Martínez)
- **Principal en**: Historia de Honduras, Educación Física
- **Colaborador en**: Ciencias Naturales

## Instrucciones de Ejecución

### Opción 1: SQL Server Management Studio (SSMS)
1. Abrir SSMS y conectarse al servidor
2. Abrir el archivo `SeedData.sql`
3. Asegurarse de que la base de datos `InternadoDB` existe
4. Ejecutar el script completo (F5)
5. Verificar el mensaje de éxito al final

### Opción 2: Azure Data Studio
1. Conectarse al servidor
2. Abrir el archivo `SeedData.sql`
3. Ejecutar el script

### Opción 3: Línea de comandos (sqlcmd)
```bash
sqlcmd -S localhost -d InternadoDB -i SeedData.sql
```

### Opción 4: Visual Studio Code con extensión SQL Server
1. Conectarse a la base de datos
2. Abrir `SeedData.sql`
3. Ejecutar el script

## Verificación de Datos

Después de ejecutar el script, verificar con estas consultas:

```sql
-- Ver usuarios creados
SELECT u.Nombre, u.Usuario, r.NombreRol
FROM sec.Usuarios u
INNER JOIN sec.Roles r ON u.RolId = r.Id;

-- Ver cursos con asignaciones
SELECT c.Nombre, COUNT(dc.Id) as TotalDocentes
FROM aca.Cursos c
LEFT JOIN DocenteCursos dc ON c.Id = dc.CursoId AND dc.Activa = 1
GROUP BY c.Nombre;

-- Ver residentes activos
SELECT COUNT(*) as TotalResidentes
FROM grl.Residentes
WHERE Estado = 'Activa';

-- Ver calificaciones registradas
SELECT COUNT(*) as TotalCalificaciones
FROM aca.Calificaciones;

-- Ver medicamentos en stock
SELECT Nombre, StockActual
FROM med.Medicamentos
ORDER BY StockActual DESC;
```

## Características del Script

✅ **Idempotente**: Puede ejecutarse múltiples veces sin duplicar datos
✅ **Seguro**: Usa `IF NOT EXISTS` para evitar conflictos
✅ **Realista**: Datos coherentes y con relaciones correctas
✅ **Completo**: Cubre todos los módulos del sistema

## Limpieza de Datos (Opcional)

Si necesitas eliminar todos los datos de prueba:

```sql
-- ADVERTENCIA: Esto eliminará TODOS los datos
DELETE FROM med.MovimientosMedicamentos;
DELETE FROM med.Consultas;
DELETE FROM aca.Calificaciones;
DELETE FROM aca.Asistencia;
DELETE FROM DocenteCursos;
DELETE FROM aca.Cursos;
DELETE FROM med.Medicamentos;
DELETE FROM grl.Residentes;
DELETE FROM grl.Habitaciones;
DELETE FROM sec.Usuarios WHERE Usuario != 'admin'; -- Mantener admin
DELETE FROM sec.LoginAttempts;
```

## Notas Importantes

- ⚠️ Este script es **solo para ambientes de desarrollo/prueba**
- ⚠️ **NO ejecutar en producción**
- 🔐 Las contraseñas son hasheadas con BCrypt
- 📅 Las fechas son relativas a la fecha de ejecución
- 🔄 El script respeta la integridad referencial

## Soporte

Si encuentras algún problema al ejecutar el script:
1. Verifica que la base de datos `InternadoDB` existe
2. Confirma que las migraciones de Entity Framework están aplicadas
3. Revisa los permisos del usuario SQL
4. Verifica que no hay datos duplicados existentes

## Hash de Contraseña

El hash BCrypt utilizado fue generado con:
```
Password: Password123
Hash: $2a$11$.MoseBQSIkcl7gdf1hvxWeX4wnD8fI.H..kcTIKI3xGZFM9kcU2vG
Hex: 0x243261243131242E4D6F7365425153496B636C376764663168767857655834776E443866492E482E2E6B6354494B493378475A464D396B6355327647
```

Generado con el proyecto `HashGenerator` incluido en el repositorio.
