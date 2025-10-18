-- =============================================
-- Script de Datos Ficticios para Sistema Internado
-- =============================================

USE InternadoDB;
GO

-- =============================================
-- 1. ROLES (si no existen)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sec.Roles WHERE NombreRol = 'Administrador')
BEGIN
    INSERT INTO sec.Roles (NombreRol, Descripcion) VALUES
    ('Administrador', 'Acceso total al sistema'),
    ('Docente', 'Gestión de cursos, calificaciones y asistencia'),
    ('Medico', 'Gestión de consultas médicas y medicamentos'),
    ('Direccion', 'Gestión de residentes y habitaciones');
END
GO

-- =============================================
-- 2. USUARIOS
-- =============================================
-- Password para todos: "Password123" (hasheado con BCrypt)
-- Hash BCrypt: $2a$11$.MoseBQSIkcl7gdf1hvxWeX4wnD8fI.H..kcTIKI3xGZFM9kcU2vG

DECLARE @AdminRolId INT = (SELECT Id FROM sec.Roles WHERE NombreRol = 'Administrador');
DECLARE @DocenteRolId INT = (SELECT Id FROM sec.Roles WHERE NombreRol = 'Docente');
DECLARE @MedicoRolId INT = (SELECT Id FROM sec.Roles WHERE NombreRol = 'Medico');
DECLARE @DireccionRolId INT = (SELECT Id FROM sec.Roles WHERE NombreRol = 'Direccion');

-- Hash en formato binario
DECLARE @PasswordHash VARBINARY(256) = 0x243261243131242E4D6F7365425153496B636C376764663168767857655834776E443866492E482E2E6B6354494B493378475A464D396B6355327647;

-- Admin
IF NOT EXISTS (SELECT 1 FROM sec.Usuarios WHERE Usuario = 'admin')
BEGIN
    INSERT INTO sec.Usuarios (Nombre, Usuario, Correo, HashContrasena, RolId, Estado, IntentosFallidos)
    VALUES ('Administrador Sistema', 'admin', 'admin@internado.com',
            @PasswordHash, @AdminRolId, 1, 0);
END

-- Docentes
IF NOT EXISTS (SELECT 1 FROM sec.Usuarios WHERE Usuario = 'docente1')
BEGIN
    INSERT INTO sec.Usuarios (Nombre, Usuario, Correo, HashContrasena, RolId, Estado, IntentosFallidos)
    VALUES
    ('María González Pérez', 'docente1', 'maria.gonzalez@internado.com',
     @PasswordHash, @DocenteRolId, 1, 0),
    ('Carlos Ramírez López', 'docente2', 'carlos.ramirez@internado.com',
     @PasswordHash, @DocenteRolId, 1, 0),
    ('Ana Martínez Silva', 'docente3', 'ana.martinez@internado.com',
     @PasswordHash, @DocenteRolId, 1, 0);
END

-- Médicos
IF NOT EXISTS (SELECT 1 FROM sec.Usuarios WHERE Usuario = 'medico1')
BEGIN
    INSERT INTO sec.Usuarios (Nombre, Usuario, Correo, HashContrasena, RolId, Estado, IntentosFallidos)
    VALUES
    ('Dr. Roberto Sánchez', 'medico1', 'roberto.sanchez@internado.com',
     @PasswordHash, @MedicoRolId, 1, 0),
    ('Dra. Patricia Torres', 'medico2', 'patricia.torres@internado.com',
     @PasswordHash, @MedicoRolId, 1, 0);
END

-- Dirección
IF NOT EXISTS (SELECT 1 FROM sec.Usuarios WHERE Usuario = 'direccion')
BEGIN
    INSERT INTO sec.Usuarios (Nombre, Usuario, Correo, HashContrasena, RolId, Estado, IntentosFallidos)
    VALUES ('Lic. Fernando Díaz', 'direccion', 'fernando.diaz@internado.com',
            @PasswordHash, @DireccionRolId, 1, 0);
END
GO

-- =============================================
-- 3. HABITACIONES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Habitaciones WHERE Numero = '101')
BEGIN
    INSERT INTO Habitaciones (Numero, Capacidad, Tipo, Estado, Piso, Edificio)
    VALUES
    ('101', 4, 'Compartida', 'Disponible', 'Piso 1', 'Edificio A'),
    ('102', 4, 'Compartida', 'Disponible', 'Piso 1', 'Edificio A'),
    ('103', 4, 'Compartida', 'Disponible', 'Piso 1', 'Edificio A'),
    ('104', 4, 'Compartida', 'Disponible', 'Piso 1', 'Edificio A'),
    ('105', 4, 'Compartida', 'Disponible', 'Piso 1', 'Edificio A'),
    ('201', 6, 'Compartida', 'Disponible', 'Piso 2', 'Edificio A'),
    ('202', 6, 'Compartida', 'Disponible', 'Piso 2', 'Edificio A'),
    ('203', 6, 'Compartida', 'Disponible', 'Piso 2', 'Edificio A'),
    ('301', 2, 'Individual', 'Disponible', 'Piso 3', 'Edificio A'),
    ('302', 2, 'Individual', 'Disponible', 'Piso 3', 'Edificio A');
END
GO

-- =============================================
-- 4. RESIDENTES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM aca.Residentes WHERE DPI = '0801199012345')
BEGIN
    INSERT INTO aca.Residentes (NombreCompleto, DPI, Tutor, Estado, FechaNacimiento, FechaIngreso, HabitacionId)
    VALUES
    -- Habitación 101
    ('Sofía Hernández Martínez', '0801199012345', 'María Martínez (Tel: 87654321)', 'Activa', '2005-03-15', '2024-01-15', 1),
    ('Luis Alberto Gómez', '0801199123456', 'Alberto Gómez (Tel: 98761234)', 'Activa', '2004-07-22', '2024-01-15', 1),
    ('Carmen Rodríguez López', '0801199234567', 'Rosa López (Tel: 87653210)', 'Activa', '2005-11-08', '2024-01-20', 1),

    -- Habitación 102
    ('José Manuel Flores', '0801199345678', 'Manuel Flores Sr. (Tel: 87652222)', 'Activa', '2004-05-30', '2024-02-01', 2),
    ('Andrea Patricia Mejía', '0801199456789', 'Patricia Mejía (Tel: 98764444)', 'Activa', '2005-09-14', '2024-02-01', 2),
    ('Roberto Carlos Díaz', '0801199567890', 'Carlos Díaz (Tel: 87656666)', 'Activa', '2004-12-25', '2024-02-05', 2),

    -- Habitación 103
    ('Diana Elizabeth Cruz', '0801199678901', 'Elizabeth Cruz (Tel: 87658888)', 'Activa', '2005-04-18', '2024-02-10', 3),
    ('Miguel Ángel Santos', '0801199789012', 'Ángel Santos (Tel: 98760000)', 'Activa', '2004-08-03', '2024-02-10', 3),
    ('Gabriela Morales Reyes', '0801199890123', 'Rosa Reyes (Tel: 87652222)', 'Activa', '2005-02-27', '2024-02-15', 3),

    -- Habitación 104
    ('Fernando José Castillo', '0801190012345', 'José Castillo (Tel: 87654444)', 'Activa', '2004-06-12', '2024-02-20', 4),
    ('Laura Melissa Vega', '0801190123456', 'Melissa Vega (Tel: 98766666)', 'Activa', '2005-10-05', '2024-02-20', 4),
    ('Daniel Alejandro Ramos', '0801190234567', 'Alejandro Ramos (Tel: 87658888)', 'Activa', '2004-03-21', '2024-02-25', 4),

    -- Habitación 105
    ('Valeria Andrea Ortiz', '0801190345678', 'Andrea Ortiz (Tel: 87650000)', 'Activa', '2005-07-09', '2024-03-01', 5),
    ('Javier Eduardo Vargas', '0801190456789', 'Eduardo Vargas (Tel: 98762222)', 'Activa', '2004-11-16', '2024-03-01', 5),
    ('Carolina Isabel Mendoza', '0801190567890', 'Isabel Mendoza (Tel: 87654444)', 'Activa', '2005-01-28', '2024-03-05', 5);
END
GO

-- =============================================
-- 5. CURSOS
-- =============================================
DECLARE @Docente1Id INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente1');
DECLARE @Docente2Id INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente2');
DECLARE @Docente3Id INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente3');

IF NOT EXISTS (SELECT 1 FROM aca.Cursos WHERE Nombre = 'Matemáticas I')
BEGIN
    INSERT INTO aca.Cursos (Nombre, DocenteId)
    VALUES
    ('Matemáticas I', @Docente1Id),
    ('Lenguaje y Literatura', @Docente2Id),
    ('Ciencias Naturales', @Docente1Id),
    ('Historia de Honduras', @Docente3Id),
    ('Inglés Básico', @Docente2Id),
    ('Educación Física', @Docente3Id);
END
GO

-- =============================================
-- 6. ASIGNACIONES DE DOCENTES A CURSOS
-- =============================================
DECLARE @Curso1 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Matemáticas I');
DECLARE @Curso2 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Lenguaje y Literatura');
DECLARE @Curso3 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Ciencias Naturales');
DECLARE @Curso4 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Historia de Honduras');
DECLARE @Curso5 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Inglés Básico');
DECLARE @Curso6 INT = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Educación Física');

DECLARE @Doc1 INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente1');
DECLARE @Doc2 INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente2');
DECLARE @Doc3 INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'docente3');

IF NOT EXISTS (SELECT 1 FROM DocenteCursos)
BEGIN
    -- Docente 1: Principal en Matemáticas y Ciencias, Colaborador en Historia
    INSERT INTO DocenteCursos (DocenteId, CursoId, Activa)
    VALUES
    (@Doc1, @Curso1, 1), -- Principal en Matemáticas
    (@Doc1, @Curso3, 1), -- Principal en Ciencias
    (@Doc1, @Curso4, 1), -- Colaborador en Historia

    -- Docente 2: Principal en Lenguaje e Inglés, Colaborador en Matemáticas
    (@Doc2, @Curso2, 1), -- Principal en Lenguaje
    (@Doc2, @Curso5, 1), -- Principal en Inglés
    (@Doc2, @Curso1, 1), -- Colaborador en Matemáticas

    -- Docente 3: Principal en Historia y Ed. Física, Colaborador en Ciencias
    (@Doc3, @Curso4, 1), -- Principal en Historia
    (@Doc3, @Curso6, 1), -- Principal en Ed. Física
    (@Doc3, @Curso3, 1); -- Colaborador en Ciencias
END
GO

-- =============================================
-- 7. CALIFICACIONES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM aca.Calificaciones)
BEGIN
    DECLARE @ResidenteId INT;
    DECLARE @CursoIdCal INT;

    -- Calificaciones para cada residente en cada curso
    -- Matemáticas I
    SET @CursoIdCal = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Matemáticas I');

    INSERT INTO aca.Calificaciones (ResidenteId, CursoId, Nota, FechaRegistro)
    SELECT Id, @CursoIdCal,
           CASE
               WHEN Id % 5 = 0 THEN 95
               WHEN Id % 4 = 0 THEN 85
               WHEN Id % 3 = 0 THEN 75
               WHEN Id % 2 = 0 THEN 68
               ELSE 55
           END,
           DATEADD(DAY, -30, GETUTCDATE())
    FROM aca.Residentes WHERE Estado = 'Activa';

    -- Lenguaje y Literatura
    SET @CursoIdCal = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Lenguaje y Literatura');

    INSERT INTO aca.Calificaciones (ResidenteId, CursoId, Nota, FechaRegistro)
    SELECT Id, @CursoIdCal,
           CASE
               WHEN Id % 4 = 0 THEN 88
               WHEN Id % 3 = 0 THEN 78
               WHEN Id % 2 = 0 THEN 70
               ELSE 62
           END,
           DATEADD(DAY, -25, GETUTCDATE())
    FROM aca.Residentes WHERE Estado = 'Activa';

    -- Ciencias Naturales
    SET @CursoIdCal = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Ciencias Naturales');

    INSERT INTO aca.Calificaciones (ResidenteId, CursoId, Nota, FechaRegistro)
    SELECT Id, @CursoIdCal,
           CASE
               WHEN Id % 5 = 0 THEN 92
               WHEN Id % 3 = 0 THEN 80
               WHEN Id % 2 = 0 THEN 65
               ELSE 58
           END,
           DATEADD(DAY, -20, GETUTCDATE())
    FROM aca.Residentes WHERE Estado = 'Activa';
END
GO

-- =============================================
-- 8. ASISTENCIAS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM aca.Asistencia)
BEGIN
    DECLARE @FechaAsist DATE = CAST(DATEADD(DAY, -10, GETDATE()) AS DATE);
    DECLARE @CursoIdAsist INT;
    DECLARE @i INT = 0;

    WHILE @i < 10
    BEGIN
        -- Matemáticas I
        SET @CursoIdAsist = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Matemáticas I');

        INSERT INTO aca.Asistencia (ResidenteId, CursoId, Fecha, Estado, Observacion)
        SELECT Id, @CursoIdAsist, @FechaAsist,
               CASE
                   WHEN Id % 10 = @i THEN 'Ausente'
                   WHEN Id % 7 = @i THEN 'Tarde'
                   ELSE 'Presente'
               END,
               CASE
                   WHEN Id % 10 = @i THEN 'Cita médica'
                   WHEN Id % 7 = @i THEN 'Problema de transporte'
                   ELSE NULL
               END
        FROM aca.Residentes WHERE Estado = 'Activa';

        -- Lenguaje
        SET @CursoIdAsist = (SELECT Id FROM aca.Cursos WHERE Nombre = 'Lenguaje y Literatura');

        INSERT INTO aca.Asistencia (ResidenteId, CursoId, Fecha, Estado, Observacion)
        SELECT Id, @CursoIdAsist, @FechaAsist,
               CASE
                   WHEN Id % 8 = @i THEN 'Ausente'
                   WHEN Id % 6 = @i THEN 'Tarde'
                   ELSE 'Presente'
               END,
               NULL
        FROM aca.Residentes WHERE Estado = 'Activa';

        SET @FechaAsist = DATEADD(DAY, 1, @FechaAsist);
        SET @i = @i + 1;
    END
END
GO

-- =============================================
-- 9. MEDICAMENTOS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM med.Medicamentos WHERE Nombre = 'Paracetamol')
BEGIN
    INSERT INTO med.Medicamentos (Nombre, Lote, FechaVencimiento, StockActual, StockMinimo)
    VALUES
    ('Paracetamol 500mg', 'LOT2024001', '2026-12-31', 500, 50),
    ('Ibuprofeno 400mg', 'LOT2024002', '2026-11-30', 300, 30),
    ('Amoxicilina 500mg', 'LOT2024003', '2025-08-31', 200, 20),
    ('Loratadina 10mg', 'LOT2024004', '2027-03-31', 150, 15),
    ('Omeprazol 20mg', 'LOT2024005', '2026-06-30', 250, 25),
    ('Vitamina C 1000mg', 'LOT2024006', '2027-12-31', 400, 40),
    ('Suero Oral', 'LOT2024007', '2026-09-30', 100, 10),
    ('Alcohol 70%', 'LOT2024008', '2027-06-30', 50, 5),
    ('Gasas Estériles', 'LOT2024009', '2028-01-31', 200, 20),
    ('Curitas', 'LOT2024010', '2027-12-31', 300, 30);
END
GO

-- =============================================
-- 10. CONSULTAS MÉDICAS
-- =============================================
DECLARE @MedicoId INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'medico1');

IF NOT EXISTS (SELECT 1 FROM med.Consultas)
BEGIN
    -- Consultas de los últimos 15 días
    INSERT INTO med.Consultas (ResidenteId, MedicoId, Fecha, Diagnostico, Tratamiento)
    SELECT TOP 8
        Id,
        @MedicoId,
        DATEADD(DAY, -(Id % 15), GETUTCDATE()),
        CASE Id % 4
            WHEN 0 THEN 'Cefalea tensional por estrés académico'
            WHEN 1 THEN 'Gastritis leve - Indicaciones dietéticas'
            WHEN 2 THEN 'Rinofaringitis viral aguda'
            ELSE 'Control rutinario - Estado de salud normal'
        END,
        CASE Id % 4
            WHEN 0 THEN 'Paracetamol 500mg cada 8 horas por 3 días. Reposo.'
            WHEN 1 THEN 'Omeprazol 20mg en ayunas por 7 días. Dieta blanda.'
            WHEN 2 THEN 'Reposo, hidratación abundante, Vitamina C 1000mg diaria'
            ELSE 'Continuar con hábitos saludables. Próximo control en 6 meses.'
        END
    FROM aca.Residentes WHERE Estado = 'Activa';
END
GO

-- =============================================
-- 11. MOVIMIENTOS DE MEDICAMENTOS
-- =============================================
DECLARE @MedUsuarioId INT = (SELECT Id FROM sec.Usuarios WHERE Usuario = 'medico1');

IF NOT EXISTS (SELECT 1 FROM med.MovimientosMedicamentos)
BEGIN
    -- Ingresos de medicamentos
    INSERT INTO med.MovimientosMedicamentos (MedicamentoId, TipoMovimiento, Cantidad, UsuarioId)
    SELECT Id, 'Entrada', StockActual, @MedUsuarioId
    FROM med.Medicamentos;

    -- Algunas salidas (dispensaciones)
    INSERT INTO med.MovimientosMedicamentos (MedicamentoId, TipoMovimiento, Cantidad, UsuarioId)
    VALUES
    (1, 'Salida', 20, @MedUsuarioId),
    (2, 'Salida', 15, @MedUsuarioId),
    (3, 'Salida', 10, @MedUsuarioId),
    (5, 'Salida', 14, @MedUsuarioId);
END
GO

PRINT '==========================================';
PRINT 'DATOS FICTICIOS CARGADOS EXITOSAMENTE';
PRINT '==========================================';
PRINT '';
PRINT 'CREDENCIALES DE ACCESO:';
PRINT '- Administrador: admin / Password123';
PRINT '- Docente 1: docente1 / Password123';
PRINT '- Docente 2: docente2 / Password123';
PRINT '- Docente 3: docente3 / Password123';
PRINT '- Médico 1: medico1 / Password123';
PRINT '- Médico 2: medico2 / Password123';
PRINT '- Dirección: direccion / Password123';
PRINT '';
PRINT 'DATOS CREADOS:';
PRINT '- 15 Residentes (en 5 habitaciones)';
PRINT '- 6 Cursos con asignaciones múltiples';
PRINT '- Calificaciones para todos los residentes';
PRINT '- Asistencias de los últimos 10 días';
PRINT '- 10 Medicamentos en inventario';
PRINT '- 8 Consultas médicas recientes';
PRINT '==========================================';
GO
