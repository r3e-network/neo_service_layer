declare module 'recharts' {
  import * as React from 'react';

  export interface LineProps {
    type?: 'basis' | 'basisClosed' | 'basisOpen' | 'linear' | 'linearClosed' | 'natural' | 'monotoneX' | 'monotoneY' | 'monotone' | 'step' | 'stepBefore' | 'stepAfter';
    dataKey: string;
    name?: string;
    stroke?: string;
    strokeWidth?: number;
    fill?: string;
    fillOpacity?: number;
    activeDot?: boolean | object;
    dot?: boolean | object;
    isAnimationActive?: boolean;
    animationDuration?: number;
    animationEasing?: 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'linear';
  }

  export interface BarProps {
    dataKey: string;
    name?: string;
    fill?: string;
    fillOpacity?: number;
    stroke?: string;
    strokeWidth?: number;
    maxBarSize?: number;
    minPointSize?: number;
    background?: object | React.ReactElement | React.ReactNode;
    isAnimationActive?: boolean;
    animationDuration?: number;
    animationEasing?: 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'linear';
  }

  export interface PieProps {
    cx?: number | string;
    cy?: number | string;
    innerRadius?: number | string;
    outerRadius?: number | string;
    startAngle?: number;
    endAngle?: number;
    minAngle?: number;
    paddingAngle?: number;
    nameKey?: string;
    dataKey: string;
    valueKey?: string;
    data?: Array<any>;
    label?: boolean | object | React.ReactElement | ((props: any) => React.ReactNode);
    labelLine?: boolean | object | React.ReactElement | ((props: any) => React.ReactNode);
    activeIndex?: number | number[];
    activeShape?: object | React.ReactElement | ((props: any) => React.ReactNode);
    isAnimationActive?: boolean;
    animationDuration?: number;
    animationEasing?: 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'linear';
  }

  export interface CellProps {
    fill?: string;
    stroke?: string;
    strokeWidth?: number;
    name?: string;
  }

  export interface XAxisProps {
    dataKey?: string;
    xAxisId?: string | number;
    width?: number;
    height?: number;
    orientation?: 'top' | 'bottom';
    type?: 'number' | 'category';
    allowDecimals?: boolean;
    allowDataOverflow?: boolean;
    tickCount?: number;
    domain?: [number, number] | [(dataMin: number) => number, (dataMax: number) => number];
    interval?: number | 'preserveStart' | 'preserveEnd' | 'preserveStartEnd';
    padding?: { left?: number; right?: number };
    minTickGap?: number;
    axisLine?: boolean | object;
    tickLine?: boolean | object;
    tickFormatter?: (value: any) => string;
    ticks?: any[];
    tick?: boolean | React.ReactElement | ((props: any) => React.ReactNode);
    mirror?: boolean;
    reversed?: boolean;
    label?: string | number | React.ReactElement | object;
    scale?: 'auto' | 'linear' | 'pow' | 'sqrt' | 'log' | 'identity' | 'time' | 'band' | 'point' | 'ordinal' | 'quantile' | 'quantize' | 'utc' | 'sequential' | 'threshold';
    unit?: string | number;
    name?: string | number;
    hide?: boolean;
  }

  export interface YAxisProps {
    dataKey?: string;
    yAxisId?: string | number;
    width?: number;
    height?: number;
    orientation?: 'left' | 'right';
    type?: 'number' | 'category';
    allowDecimals?: boolean;
    allowDataOverflow?: boolean;
    tickCount?: number;
    domain?: [number, number] | [(dataMin: number) => number, (dataMax: number) => number];
    interval?: number | 'preserveStart' | 'preserveEnd' | 'preserveStartEnd';
    padding?: { top?: number; bottom?: number };
    minTickGap?: number;
    axisLine?: boolean | object;
    tickLine?: boolean | object;
    tickFormatter?: (value: any) => string;
    ticks?: any[];
    tick?: boolean | React.ReactElement | ((props: any) => React.ReactNode);
    mirror?: boolean;
    reversed?: boolean;
    label?: string | number | React.ReactElement | object;
    scale?: 'auto' | 'linear' | 'pow' | 'sqrt' | 'log' | 'identity' | 'time' | 'band' | 'point' | 'ordinal' | 'quantile' | 'quantize' | 'utc' | 'sequential' | 'threshold';
    unit?: string | number;
    name?: string | number;
    hide?: boolean;
  }

  export interface TooltipProps {
    content?: React.ReactElement | ((props: any) => React.ReactNode);
    formatter?: (value: any, name: string, props: any) => [string, string] | string;
    labelFormatter?: (label: any) => React.ReactNode;
    itemSorter?: (item: any) => number;
    isAnimationActive?: boolean;
    animationDuration?: number;
    animationEasing?: 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'linear';
    active?: boolean;
    position?: { x?: number; y?: number };
    coordinate?: { x?: number; y?: number };
    cursor?: boolean | object | React.ReactElement;
    viewBox?: { x?: number; y?: number; width?: number; height?: number };
    allowEscapeViewBox?: { x?: boolean; y?: boolean };
    offset?: number;
    wrapperStyle?: object;
    contentStyle?: object;
    itemStyle?: object;
    labelStyle?: object;
    trigger?: 'hover' | 'click';
    filterNull?: boolean;
  }

  export interface LegendProps {
    content?: React.ReactElement | ((props: any) => React.ReactNode);
    iconSize?: number;
    iconType?: 'line' | 'square' | 'rect' | 'circle' | 'cross' | 'diamond' | 'star' | 'triangle' | 'wye';
    layout?: 'horizontal' | 'vertical';
    align?: 'left' | 'center' | 'right';
    verticalAlign?: 'top' | 'middle' | 'bottom';
    width?: number;
    height?: number;
    margin?: { top?: number; right?: number; bottom?: number; left?: number };
    wrapperStyle?: object;
    payload?: Array<{ value: any; id: string; type: string; color: string }>;
    formatter?: (value: any, entry: any, index: number) => React.ReactNode;
    onClick?: (event: any) => void;
    onMouseEnter?: (event: any) => void;
    onMouseLeave?: (event: any) => void;
  }

  export interface CartesianGridProps {
    x?: number;
    y?: number;
    width?: number;
    height?: number;
    horizontal?: boolean;
    vertical?: boolean;
    horizontalPoints?: number[];
    verticalPoints?: number[];
    horizontalCoordinatesGenerator?: (props: any) => number[];
    verticalCoordinatesGenerator?: (props: any) => number[];
    strokeDasharray?: string;
  }

  export interface ResponsiveContainerProps {
    aspect?: number;
    width?: string | number;
    height?: string | number;
    minWidth?: string | number;
    minHeight?: string | number;
    maxHeight?: string | number;
    debounce?: number;
  }

  export interface LineChartProps {
    layout?: 'horizontal' | 'vertical';
    syncId?: string;
    width?: number;
    height?: number;
    data?: Array<any>;
    margin?: { top?: number; right?: number; bottom?: number; left?: number };
    className?: string;
    style?: object;
    children?: React.ReactNode;
  }

  export interface BarChartProps {
    layout?: 'horizontal' | 'vertical';
    syncId?: string;
    width?: number;
    height?: number;
    data?: Array<any>;
    margin?: { top?: number; right?: number; bottom?: number; left?: number };
    barCategoryGap?: number | string;
    barGap?: number | string;
    barSize?: number;
    maxBarSize?: number;
    stackOffset?: 'expand' | 'none' | 'wiggle' | 'silhouette' | 'sign';
    className?: string;
    style?: object;
    children?: React.ReactNode;
  }

  export interface PieChartProps {
    width?: number;
    height?: number;
    margin?: { top?: number; right?: number; bottom?: number; left?: number };
    className?: string;
    style?: object;
    children?: React.ReactNode;
  }

  export const Line: React.FC<LineProps>;
  export const Bar: React.FC<BarProps>;
  export const Pie: React.FC<PieProps>;
  export const Cell: React.FC<CellProps>;
  export const XAxis: React.FC<XAxisProps>;
  export const YAxis: React.FC<YAxisProps>;
  export const Tooltip: React.FC<TooltipProps>;
  export const Legend: React.FC<LegendProps>;
  export const CartesianGrid: React.FC<CartesianGridProps>;
  export const ResponsiveContainer: React.FC<ResponsiveContainerProps>;
  export const LineChart: React.FC<LineChartProps>;
  export const BarChart: React.FC<BarChartProps>;
  export const PieChart: React.FC<PieChartProps>;
}
