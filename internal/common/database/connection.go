package database

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/common/config"
)

// Connection represents a database connection
type Connection interface {
	// Close closes the database connection
	Close() error

	// Ping pings the database to check if it's available
	Ping(ctx context.Context) error

	// Exec executes a query without returning any rows
	Exec(ctx context.Context, query string, args ...interface{}) (sql.Result, error)

	// Query executes a query that returns rows
	Query(ctx context.Context, query string, args ...interface{}) (*sql.Rows, error)

	// QueryRow executes a query that returns at most one row
	QueryRow(ctx context.Context, query string, args ...interface{}) *sql.Row

	// Begin starts a new transaction
	Begin(ctx context.Context) (Transaction, error)
}

// Transaction represents a database transaction
type Transaction interface {
	// Commit commits the transaction
	Commit() error

	// Rollback rolls back the transaction
	Rollback() error

	// Exec executes a query without returning any rows
	Exec(ctx context.Context, query string, args ...interface{}) (sql.Result, error)

	// Query executes a query that returns rows
	Query(ctx context.Context, query string, args ...interface{}) (*sql.Rows, error)

	// QueryRow executes a query that returns at most one row
	QueryRow(ctx context.Context, query string, args ...interface{}) *sql.Row
}

// DBConnection implements the Connection interface
type DBConnection struct {
	db *sql.DB
}

// DBTransaction implements the Transaction interface
type DBTransaction struct {
	tx *sql.Tx
}

// ConnectDB connects to the database
func ConnectDB(cfg *config.DatabaseConfig) (Connection, error) {
	// Build the connection string based on the database type
	var connStr string
	switch cfg.Driver {
	case "postgres":
		connStr = fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=%s",
			cfg.Host,
			cfg.Port,
			cfg.User,
			cfg.Password,
			cfg.Name,
			cfg.SSLMode,
		)
	default:
		log.Fatalf("Unsupported database driver: %s", cfg.Driver)
	}

	// Open the database connection
	db, err := sql.Open(cfg.Driver, connStr)
	if err != nil {
		return nil, fmt.Errorf("failed to open database connection: %w", err)
	}

	// Configure the connection pool
	db.SetMaxOpenConns(25)
	db.SetMaxIdleConns(5)
	db.SetConnMaxLifetime(time.Hour)

	// Ping the database to verify the connection
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	if err := db.PingContext(ctx); err != nil {
		db.Close()
		return nil, fmt.Errorf("failed to ping database: %w", err)
	}

	return &DBConnection{db: db}, nil
}

// Close closes the database connection
func (c *DBConnection) Close() error {
	return c.db.Close()
}

// Ping pings the database to check if it's available
func (c *DBConnection) Ping(ctx context.Context) error {
	return c.db.PingContext(ctx)
}

// Exec executes a query without returning any rows
func (c *DBConnection) Exec(ctx context.Context, query string, args ...interface{}) (sql.Result, error) {
	return c.db.ExecContext(ctx, query, args...)
}

// Query executes a query that returns rows
func (c *DBConnection) Query(ctx context.Context, query string, args ...interface{}) (*sql.Rows, error) {
	return c.db.QueryContext(ctx, query, args...)
}

// QueryRow executes a query that returns at most one row
func (c *DBConnection) QueryRow(ctx context.Context, query string, args ...interface{}) *sql.Row {
	return c.db.QueryRowContext(ctx, query, args...)
}

// Begin starts a new transaction
func (c *DBConnection) Begin(ctx context.Context) (Transaction, error) {
	tx, err := c.db.BeginTx(ctx, nil)
	if err != nil {
		return nil, err
	}
	return &DBTransaction{tx: tx}, nil
}

// Commit commits the transaction
func (t *DBTransaction) Commit() error {
	return t.tx.Commit()
}

// Rollback rolls back the transaction
func (t *DBTransaction) Rollback() error {
	return t.tx.Rollback()
}

// Exec executes a query without returning any rows
func (t *DBTransaction) Exec(ctx context.Context, query string, args ...interface{}) (sql.Result, error) {
	return t.tx.ExecContext(ctx, query, args...)
}

// Query executes a query that returns rows
func (t *DBTransaction) Query(ctx context.Context, query string, args ...interface{}) (*sql.Rows, error) {
	return t.tx.QueryContext(ctx, query, args...)
}

// QueryRow executes a query that returns at most one row
func (t *DBTransaction) QueryRow(ctx context.Context, query string, args ...interface{}) *sql.Row {
	return t.tx.QueryRowContext(ctx, query, args...)
}
