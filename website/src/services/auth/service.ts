import { EventEmitter } from 'events';

export interface WalletInfo {
  address: string;
  publicKey: string;
  network: string;
}

export interface AuthenticationResult {
  token: string;
  expiresAt: number;
  walletInfo: WalletInfo;
}

export class AuthenticationService extends EventEmitter {
  private static instance: AuthenticationService;
  private wallet: WalletInfo | null = null;
  private token: string | null = null;
  private tokenExpiry: number = 0;
  private refreshTimeout: NodeJS.Timeout | null = null;

  private constructor() {
    super();
    this.checkStoredAuth();
  }

  public static getInstance(): AuthenticationService {
    if (!AuthenticationService.instance) {
      AuthenticationService.instance = new AuthenticationService();
    }
    return AuthenticationService.instance;
  }

  public async connectWallet(): Promise<WalletInfo> {
    try {
      // Check if Neo Line extension is available
      const neoLine = (window as any).NEOLine;
      if (!neoLine) {
        throw new Error('Neo Line extension not found');
      }

      // Request connection
      const { address, publicKey } = await neoLine.getAccount();
      const { networkMagic } = await neoLine.getNetworks();
      
      const network = this.getNetworkName(networkMagic);
      
      this.wallet = { address, publicKey, network };
      this.emit('wallet_connected', this.wallet);
      
      return this.wallet;
    } catch (error) {
      console.error('Error connecting wallet:', error);
      throw error;
    }
  }

  public async authenticate(): Promise<AuthenticationResult> {
    if (!this.wallet) {
      throw new Error('Wallet not connected');
    }

    try {
      // For demo purposes, we're just creating a simple token
      // In a real application, this would involve a server-side authentication process
      const token = `auth_${this.wallet.address}_${Date.now()}`;
      const expiresAt = Date.now() + 3600000; // 1 hour
      
      this.token = token;
      this.tokenExpiry = expiresAt;
      
      // Store authentication in localStorage
      this.storeAuth();
      
      // Set up token refresh
      this.setupTokenRefresh();
      
      this.emit('authenticated', { token, expiresAt, walletInfo: this.wallet });
      
      return { token, expiresAt, walletInfo: this.wallet };
    } catch (error) {
      console.error('Authentication error:', error);
      throw error;
    }
  }

  public isAuthenticated(): boolean {
    return !!this.token && Date.now() < this.tokenExpiry;
  }

  public getWallet(): WalletInfo | null {
    return this.wallet;
  }

  public getToken(): string | null {
    return this.token;
  }

  public disconnect(): void {
    this.wallet = null;
    this.token = null;
    this.tokenExpiry = 0;
    
    if (this.refreshTimeout) {
      clearTimeout(this.refreshTimeout);
      this.refreshTimeout = null;
    }
    
    // Clear stored authentication
    localStorage.removeItem('neo_auth');
    
    this.emit('disconnected');
  }

  private setupTokenRefresh(): void {
    if (this.refreshTimeout) {
      clearTimeout(this.refreshTimeout);
    }
    
    const timeUntilRefresh = this.tokenExpiry - Date.now() - 300000; // Refresh 5 minutes before expiry
    
    if (timeUntilRefresh > 0) {
      this.refreshTimeout = setTimeout(() => this.refreshToken(), timeUntilRefresh);
    } else {
      // Token is already expired or about to expire
      this.refreshToken();
    }
  }

  private async refreshToken(): Promise<void> {
    if (!this.wallet) {
      return;
    }
    
    try {
      // In a real application, this would involve a server-side token refresh
      const token = `auth_${this.wallet.address}_${Date.now()}`;
      const expiresAt = Date.now() + 3600000; // 1 hour
      
      this.token = token;
      this.tokenExpiry = expiresAt;
      
      // Store updated authentication
      this.storeAuth();
      
      // Set up next token refresh
      this.setupTokenRefresh();
      
      this.emit('token_refreshed', { token, expiresAt });
    } catch (error) {
      console.error('Token refresh error:', error);
      // If refresh fails, user needs to re-authenticate
      this.disconnect();
    }
  }

  private storeAuth(): void {
    if (this.wallet && this.token) {
      const authData = {
        wallet: this.wallet,
        token: this.token,
        tokenExpiry: this.tokenExpiry
      };
      
      localStorage.setItem('neo_auth', JSON.stringify(authData));
    }
  }

  private checkStoredAuth(): void {
    try {
      const storedAuth = localStorage.getItem('neo_auth');
      
      if (storedAuth) {
        const { wallet, token, tokenExpiry } = JSON.parse(storedAuth);
        
        // Check if token is still valid
        if (Date.now() < tokenExpiry) {
          this.wallet = wallet;
          this.token = token;
          this.tokenExpiry = tokenExpiry;
          
          // Set up token refresh
          this.setupTokenRefresh();
          
          this.emit('restored_session', { wallet, token });
        } else {
          // Token expired, clear storage
          localStorage.removeItem('neo_auth');
        }
      }
    } catch (error) {
      console.error('Error checking stored authentication:', error);
      // If there's an error, clear storage to be safe
      localStorage.removeItem('neo_auth');
    }
  }

  private getNetworkName(networkMagic: number): string {
    switch (networkMagic) {
      case 844378958:
        return 'MainNet';
      case 877933390:
        return 'TestNet';
      default:
        return 'Unknown';
    }
  }
}