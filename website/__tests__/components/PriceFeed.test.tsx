// @ts-ignore - Allow synthetic default imports
import React from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { act } from 'react-dom/test-utils'
import PriceFeed from '../../src/components/PriceFeed'

const mockPriceData = {
  price: 42.50,
  timestamp: new Date().toISOString(),
  confidence: 0.95
}

describe('PriceFeed Component', () => {
  beforeEach(() => {
    // Reset fetch mock before each test
    // @ts-ignore - Mock implementation for fetch
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve(mockPriceData),
        status: 200,
        statusText: 'OK',
        headers: new Headers(),
        redirected: false,
        type: 'basic',
        url: '',
        clone: () => ({} as Response),
        body: null,
        bodyUsed: false,
        arrayBuffer: () => Promise.resolve(new ArrayBuffer(0)),
        blob: () => Promise.resolve(new Blob()),
        formData: () => Promise.resolve(new FormData()),
        text: () => Promise.resolve('')
      } as Response)
    )
  })

  it('renders the price feed component', async () => {
    render(<PriceFeed symbol="NEO/USD" />)
    
    // Initially shows loading
    expect(screen.getByRole('progressbar')).toBeInTheDocument()
    
    // Wait for price data to load
    await waitFor(() => {
      expect(screen.getByText(/NEO\/USD/i)).toBeInTheDocument()
      expect(screen.getByText(/42.50/)).toBeInTheDocument()
    })
  })

  it('displays loading state initially', () => {
    render(<PriceFeed symbol="NEO/USD" />)
    expect(screen.getByRole('progressbar')).toBeInTheDocument()
  })

  it('fetches and displays price data', async () => {
    render(<PriceFeed symbol="NEO/USD" />)
    
    await waitFor(() => {
      expect(screen.getByText(/42.50/)).toBeInTheDocument()
      expect(screen.getByText(/confidence/i)).toBeInTheDocument()
    })
  })

  it('handles fetch errors', async () => {
    // @ts-ignore - Mock implementation for fetch error
    global.fetch = jest.fn(() =>
      Promise.reject(new Error('Failed to fetch'))
    )

    render(<PriceFeed symbol="NEO/USD" />)
    
    await waitFor(() => {
      expect(screen.getByText(/Failed to fetch price data/i)).toBeInTheDocument()
    })
  })

  it('updates price data when refresh button is clicked', async () => {
    const user = userEvent.setup()
    render(<PriceFeed symbol="NEO/USD" />)
    
    // Wait for initial data to load
    await waitFor(() => {
      expect(screen.getByText(/42.50/)).toBeInTheDocument()
    })

    // Update mock data for refresh
    const newPriceData = { ...mockPriceData, price: 43.00 }
    // @ts-ignore - Mock implementation for fetch
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve(newPriceData),
        status: 200,
        statusText: 'OK',
        headers: new Headers(),
        redirected: false,
        type: 'basic',
        url: '',
        clone: () => ({} as Response),
        body: null,
        bodyUsed: false,
        arrayBuffer: () => Promise.resolve(new ArrayBuffer(0)),
        blob: () => Promise.resolve(new Blob()),
        formData: () => Promise.resolve(new FormData()),
        text: () => Promise.resolve('')
      } as Response)
    )

    // Click refresh button
    await act(async () => {
      await user.click(screen.getByTestId('RefreshIcon'))
    })

    // Wait for new data
    await waitFor(() => {
      expect(screen.getByText(/43.00/)).toBeInTheDocument()
    })
  })
})