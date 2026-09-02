SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS controlelab
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE controlelab;

CREATE TABLE IF NOT EXISTS tools (
  id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  code VARCHAR(50) NOT NULL UNIQUE,
  name VARCHAR(120) NOT NULL,
  description VARCHAR(255),
  total_quantity INT UNSIGNED NOT NULL DEFAULT 1,
  available_quantity INT UNSIGNED NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT chk_tool_total_quantity CHECK (total_quantity > 0),
  CONSTRAINT chk_tool_available_quantity CHECK (
    available_quantity <= total_quantity
  )
);

CREATE TABLE IF NOT EXISTS loans (
  id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  tool_id INT UNSIGNED NOT NULL,
  student_name VARCHAR(120) NOT NULL,
  student_registration VARCHAR(40),
  responsible_name VARCHAR(120) NOT NULL,
  quantity INT UNSIGNED NOT NULL DEFAULT 1,
  loaned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  returned_at DATETIME NULL,
  return_responsible_name VARCHAR(120) NULL,
  status ENUM('EMPRESTADO', 'DEVOLVIDO') NOT NULL DEFAULT 'EMPRESTADO',
  notes VARCHAR(255) NULL,
  CONSTRAINT fk_loan_tool FOREIGN KEY (tool_id) REFERENCES tools(id),
  CONSTRAINT chk_loan_quantity CHECK (quantity > 0),
  INDEX idx_loans_status (status),
  INDEX idx_loans_tool (tool_id)
);

INSERT IGNORE INTO tools
  (code, name, description, total_quantity, available_quantity)
VALUES
  ('FER-001', 'Chave Phillips', 'Chave média para manutenção', 5, 5),
  ('EQP-001', 'Multímetro Digital', 'Equipamento para medições elétricas', 3, 3),
  ('FER-002', 'Alicate de Corte', 'Alicate para fios e cabos', 4, 4);
