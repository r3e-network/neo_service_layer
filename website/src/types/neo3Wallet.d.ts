declare interface Neo3Wallet {
  signMessage(message: string): Promise<string>;
  getAccount(): Promise<{
    address: string;
    publicKey: string;
  }>;
  getBalance(): Promise<{
    NEO: string;
    GAS: string;
  }>;
  getNetwork(): Promise<string>;
}

declare interface Window {
  neo3Wallet?: Neo3Wallet;
} 