declare module 'chart.js' {
  export type ChartType = 'line' | 'bar' | 'pie' | 'doughnut' | 'radar' | 'polarArea' | 'bubble' | 'scatter';
  
  export interface ChartOptions {
    responsive?: boolean;
    maintainAspectRatio?: boolean;
    scales?: {
      x?: {
        type?: string;
        time?: {
          unit?: string;
        };
        title?: {
          display?: boolean;
          text?: string;
        };
      };
      y?: {
        beginAtZero?: boolean;
        title?: {
          display?: boolean;
          text?: string;
        };
      };
    };
    plugins?: {
      legend?: {
        display?: boolean;
        position?: 'top' | 'bottom' | 'left' | 'right';
      };
      title?: {
        display?: boolean;
        text?: string;
      };
      tooltip?: {
        enabled?: boolean;
      };
    };
  }

  export interface ChartData {
    labels?: string[];
    datasets: {
      label?: string;
      data: number[] | { x: number; y: number }[];
      backgroundColor?: string | string[];
      borderColor?: string | string[];
      borderWidth?: number;
      fill?: boolean;
      tension?: number;
    }[];
  }

  export interface ChartProps {
    type?: ChartType;
    data: ChartData;
    options?: ChartOptions;
    height?: number;
    width?: number;
  }

  export class Chart {
    static register(...items: any[]): void;
  }

  export const CategoryScale: any;
  export const LinearScale: any;
  export const PointElement: any;
  export const LineElement: any;
  export const Title: any;
  export const Tooltip: any;
  export const Legend: any;
  export const TimeScale: any;
}
