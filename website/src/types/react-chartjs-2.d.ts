declare module 'react-chartjs-2' {
  import { ChartData, ChartOptions } from 'chart.js';
  import * as React from 'react';

  interface ChartComponentProps {
    data: ChartData;
    options?: ChartOptions;
    height?: number;
    width?: number;
  }

  export const Line: React.FC<ChartComponentProps>;
  export const Bar: React.FC<ChartComponentProps>;
  export const Pie: React.FC<ChartComponentProps>;
  export const Doughnut: React.FC<ChartComponentProps>;
  export const Radar: React.FC<ChartComponentProps>;
  export const PolarArea: React.FC<ChartComponentProps>;
  export const Bubble: React.FC<ChartComponentProps>;
  export const Scatter: React.FC<ChartComponentProps>;
}
