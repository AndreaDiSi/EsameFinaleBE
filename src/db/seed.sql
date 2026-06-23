-- AutoConfig API – seed data
-- Mirrors DbSeeder.cs exactly: same fixed GUIDs, same data.
-- Passwords are hashed with pgcrypto's crypt() using Blowfish (bf) cost 11,
-- which produces $2a$ hashes fully compatible with BCrypt.Net.BCrypt.Verify().
--
-- After running this file the .NET API's DbSeeder.SeedAsync() will detect that
-- Users already exist and return early — no duplicate-key errors.
--
-- Requires: pgcrypto extension (enabled by schema.sql)

\set ON_ERROR_STOP on

-- ── Users ─────────────────────────────────────────────────────────────────────
-- Passwords: admin123 | mario123 | giulia123
INSERT INTO "Users" ("Id", "Email", "Name", "PasswordHash", "Role", "CreatedAt") VALUES
('44444444-0000-0000-0000-000000000001', 'admin@autoconfig.it', 'Admin Sistema',  crypt('admin123',  gen_salt('bf', 11)), 'Admin', '2024-01-01 00:00:00+00'),
('44444444-0000-0000-0000-000000000002', 'mario@example.com',   'Mario Rossi',    crypt('mario123',  gen_salt('bf', 11)), 'User',  '2024-02-15 00:00:00+00'),
('44444444-0000-0000-0000-000000000003', 'giulia@example.com',  'Giulia Bianchi', crypt('giulia123', gen_salt('bf', 11)), 'User',  '2024-03-10 00:00:00+00');

-- ── Car models ────────────────────────────────────────────────────────────────
INSERT INTO "CarModels" ("Id", "Name", "Brand", "Category", "BasePrice", "Description", "ImageColor") VALUES
('11111111-0000-0000-0000-000000000001', 'Serie 3',  'BMW',        'Sedan',     42900, 'La berlina sportiva per eccellenza, con un equilibrio perfetto tra prestazioni e comfort.', '#1a1a2e'),
('11111111-0000-0000-0000-000000000002', 'Q5',       'Audi',       'Suv',       56900, 'SUV premium con tecnologia all''avanguardia e design raffinato.',                           '#16213e'),
('11111111-0000-0000-0000-000000000003', 'Classe C', 'Mercedes',   'Sedan',     46500, 'Eleganza senza compromessi con il massimo del lusso tedesco.',                               '#0f3460'),
('11111111-0000-0000-0000-000000000004', 'Golf',     'Volkswagen', 'Hatchback', 28900, 'L''icona del segmento compatto, affidabile e versatile.',                                   '#533483'),
('11111111-0000-0000-0000-000000000005', 'Cayenne',  'Porsche',    'Suv',       89900, 'Il SUV sportivo definitivo, prestazioni da supercar con la praticità di un SUV.',            '#e94560');

-- ── Motorizations ─────────────────────────────────────────────────────────────
INSERT INTO "Motorizations" ("Id", "ModelId", "Name", "FuelType", "Power", "Torque", "Acceleration", "Consumption", "Price") VALUES
-- BMW Serie 3
('22222222-0000-0000-0000-000000000001', '11111111-0000-0000-0000-000000000001', '318i',      'Petrol', 156, 250,  8.4, '6.1 L/100km',  0),
('22222222-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000001', '320d',      'Diesel', 190, 400,  7.1, '4.5 L/100km',  2500),
('22222222-0000-0000-0000-000000000003', '11111111-0000-0000-0000-000000000001', '330i',      'Petrol', 258, 400,  5.8, '6.9 L/100km',  6800),
('22222222-0000-0000-0000-000000000004', '11111111-0000-0000-0000-000000000001', '330e',      'Hybrid', 292, 420,  5.9, '1.8 L/100km',  9200),
-- Audi Q5
('22222222-0000-0000-0000-000000000005', '11111111-0000-0000-0000-000000000002', '35 TDI',    'Diesel', 163, 370,  9.0, '5.2 L/100km',  0),
('22222222-0000-0000-0000-000000000006', '11111111-0000-0000-0000-000000000002', '40 TDI',    'Diesel', 204, 400,  7.5, '5.8 L/100km',  3200),
('22222222-0000-0000-0000-000000000007', '11111111-0000-0000-0000-000000000002', '45 TFSI',   'Petrol', 265, 370,  5.9, '7.3 L/100km',  5900),
('22222222-0000-0000-0000-000000000008', '11111111-0000-0000-0000-000000000002', '55 TFSI e', 'Hybrid', 367, 500,  5.3, '2.2 L/100km',  12500),
-- Mercedes Classe C
('22222222-0000-0000-0000-000000000009', '11111111-0000-0000-0000-000000000003', 'C 180',     'Petrol', 170, 270,  8.0, '6.4 L/100km',  0),
('22222222-0000-0000-0000-000000000010', '11111111-0000-0000-0000-000000000003', 'C 220 d',   'Diesel', 200, 440,  7.1, '4.7 L/100km',  2800),
('22222222-0000-0000-0000-000000000011', '11111111-0000-0000-0000-000000000003', 'C 300',     'Petrol', 258, 400,  6.0, '7.0 L/100km',  7100),
('22222222-0000-0000-0000-000000000012', '11111111-0000-0000-0000-000000000003', 'C 300 e',   'Hybrid', 313, 550,  5.7, '1.5 L/100km',  10800),
-- VW Golf
('22222222-0000-0000-0000-000000000013', '11111111-0000-0000-0000-000000000004', '1.0 eTSI',  'Petrol', 110, 200, 10.5, '5.4 L/100km',  0),
('22222222-0000-0000-0000-000000000014', '11111111-0000-0000-0000-000000000004', '1.5 eTSI',  'Petrol', 150, 250,  8.5, '5.9 L/100km',  1800),
('22222222-0000-0000-0000-000000000015', '11111111-0000-0000-0000-000000000004', '2.0 TDI',   'Diesel', 150, 360,  8.6, '4.3 L/100km',  2200),
('22222222-0000-0000-0000-000000000016', '11111111-0000-0000-0000-000000000004', 'GTE',       'Hybrid', 245, 400,  6.7, '1.4 L/100km',  8500),
-- Porsche Cayenne
('22222222-0000-0000-0000-000000000017', '11111111-0000-0000-0000-000000000005', 'V6 3.0T',   'Petrol', 340, 450,  6.2, '9.4 L/100km',  0),
('22222222-0000-0000-0000-000000000018', '11111111-0000-0000-0000-000000000005', 'S V8 2.9T', 'Petrol', 440, 550,  5.0, '10.6 L/100km', 18500),
('22222222-0000-0000-0000-000000000019', '11111111-0000-0000-0000-000000000005', 'E-Hybrid',  'Hybrid', 470, 700,  4.7, '3.2 L/100km',  22000),
('22222222-0000-0000-0000-000000000020', '11111111-0000-0000-0000-000000000005', 'Turbo V8',  'Petrol', 650, 800,  3.7, '12.4 L/100km', 56000);

-- ── Car options ───────────────────────────────────────────────────────────────
INSERT INTO "CarOptions" ("Id", "Name", "Description", "Category", "Price", "Color") VALUES
-- Colors (opt1–opt5)
('33333333-0000-0000-0000-000000000001', 'Bianco Alpine',            'Bianco metallizzato brillante',                          'Color',      0,    '#F5F5F5'),
('33333333-0000-0000-0000-000000000002', 'Nero Zaffiro',             'Nero metallizzato profondo',                             'Color',      1200, '#1a1a1a'),
('33333333-0000-0000-0000-000000000003', 'Blu Portimao',             'Blu metallizzato intenso',                               'Color',      1200, '#1E3A5F'),
('33333333-0000-0000-0000-000000000004', 'Rosso Aventurin',          'Rosso metallizzato vibrante',                            'Color',      1500, '#8B1A1A'),
('33333333-0000-0000-0000-000000000005', 'Grigio Mineral',           'Grigio opaco effetto pietra',                            'Color',      1800, '#708090'),
-- Interiors (opt6–opt8)
('33333333-0000-0000-0000-000000000006', 'Interni Standard Nero',    'Interni in tessuto nero con cuciture a contrasto',        'Interior',   0,    NULL),
('33333333-0000-0000-0000-000000000007', 'Interni Dakota Beige',     'Sedili in pelle Dakota color beige',                     'Interior',   2800, NULL),
('33333333-0000-0000-0000-000000000008', 'Interni Merino Cognac',    'Pelle Merino pieno fiore color cognac',                  'Interior',   5200, NULL),
-- Technology (opt9–opt13)
('33333333-0000-0000-0000-000000000009', 'Sistema di Navigazione',   'GPS integrato con aggiornamenti mappe',                  'Technology', 1800, NULL),
('33333333-0000-0000-0000-000000000010', 'Pacchetto Parcheggio',     'Sensori parcheggio anteriori e posteriori + telecamera',  'Technology', 900,  NULL),
('33333333-0000-0000-0000-000000000011', 'Head-Up Display',          'Proiezione velocità e navigazione sul parabrezza',        'Technology', 1200, NULL),
('33333333-0000-0000-0000-000000000012', 'Illuminazione Ambiente',   'Illuminazione ambientale interna a 64 colori',            'Technology', 500,  NULL),
('33333333-0000-0000-0000-000000000013', 'Harman Kardon Audio',      'Impianto audio premium 17 altoparlanti',                  'Technology', 2200, NULL),
-- Safety (opt14–opt16)
('33333333-0000-0000-0000-000000000014', 'Pacchetto Driver Assist',  'ACC, Lane Keeping, Emergency Brake',                     'Safety',     2400, NULL),
('33333333-0000-0000-0000-000000000015', 'Monitoraggio Angolo Cieco','Rilevamento veicoli negli angoli ciechi',                 'Safety',     800,  NULL),
('33333333-0000-0000-0000-000000000016', 'Night Vision',             'Sistema visione notturna con rilevamento pedoni',         'Safety',     2100, NULL),
-- Comfort (opt17–opt21)
('33333333-0000-0000-0000-000000000017', 'Sedili Riscaldati',        'Riscaldamento anteriore e posteriore',                   'Comfort',    600,  NULL),
('33333333-0000-0000-0000-000000000018', 'Sedili Ventilati',         'Ventilazione sedili anteriori',                          'Comfort',    900,  NULL),
('33333333-0000-0000-0000-000000000019', 'Tetto Panoramico',         'Tetto apribile in vetro panoramico',                     'Comfort',    1900, NULL),
('33333333-0000-0000-0000-000000000020', 'Keyless Entry',            'Accesso e avviamento senza chiave',                      'Comfort',    700,  NULL),
('33333333-0000-0000-0000-000000000021', 'Portellone Elettrico',     'Apertura/chiusura elettrica del bagagliaio',             'Comfort',    1100, NULL);

-- ── Option incompatibilities ──────────────────────────────────────────────────
-- Colors opt1–opt5 are mutually exclusive: all 20 directed pairs.
-- Interiors opt6–opt8 are mutually exclusive: all 6 directed pairs.
INSERT INTO "OptionIncompatibilities" ("IncompatibleWithId", "IncompatibleWithMeId") VALUES
-- opt1 vs. opt2,3,4,5
('33333333-0000-0000-0000-000000000001', '33333333-0000-0000-0000-000000000002'),
('33333333-0000-0000-0000-000000000001', '33333333-0000-0000-0000-000000000003'),
('33333333-0000-0000-0000-000000000001', '33333333-0000-0000-0000-000000000004'),
('33333333-0000-0000-0000-000000000001', '33333333-0000-0000-0000-000000000005'),
-- opt2 vs. opt1,3,4,5
('33333333-0000-0000-0000-000000000002', '33333333-0000-0000-0000-000000000001'),
('33333333-0000-0000-0000-000000000002', '33333333-0000-0000-0000-000000000003'),
('33333333-0000-0000-0000-000000000002', '33333333-0000-0000-0000-000000000004'),
('33333333-0000-0000-0000-000000000002', '33333333-0000-0000-0000-000000000005'),
-- opt3 vs. opt1,2,4,5
('33333333-0000-0000-0000-000000000003', '33333333-0000-0000-0000-000000000001'),
('33333333-0000-0000-0000-000000000003', '33333333-0000-0000-0000-000000000002'),
('33333333-0000-0000-0000-000000000003', '33333333-0000-0000-0000-000000000004'),
('33333333-0000-0000-0000-000000000003', '33333333-0000-0000-0000-000000000005'),
-- opt4 vs. opt1,2,3,5
('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000001'),
('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000002'),
('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000003'),
('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000005'),
-- opt5 vs. opt1,2,3,4
('33333333-0000-0000-0000-000000000005', '33333333-0000-0000-0000-000000000001'),
('33333333-0000-0000-0000-000000000005', '33333333-0000-0000-0000-000000000002'),
('33333333-0000-0000-0000-000000000005', '33333333-0000-0000-0000-000000000003'),
('33333333-0000-0000-0000-000000000005', '33333333-0000-0000-0000-000000000004'),
-- opt6 vs. opt7,8
('33333333-0000-0000-0000-000000000006', '33333333-0000-0000-0000-000000000007'),
('33333333-0000-0000-0000-000000000006', '33333333-0000-0000-0000-000000000008'),
-- opt7 vs. opt6,8
('33333333-0000-0000-0000-000000000007', '33333333-0000-0000-0000-000000000006'),
('33333333-0000-0000-0000-000000000007', '33333333-0000-0000-0000-000000000008'),
-- opt8 vs. opt6,7
('33333333-0000-0000-0000-000000000008', '33333333-0000-0000-0000-000000000006'),
('33333333-0000-0000-0000-000000000008', '33333333-0000-0000-0000-000000000007');
