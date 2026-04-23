import type { ComponentType } from "./component.types";

export interface IPriceAlert {
  id: string;
  componentId: string;
  componentType: ComponentType;
  thresholdPercent: number;
  lastNotifiedPrice: number;
  createdAt: string;
}

export interface ICreatePriceAlertRequest {
  componentId: string;
  componentType: ComponentType;
  thresholdPercent: number;
}

export interface IUserPriceAlert {
  id: string;
  componentId: string;
  componentType: ComponentType;
  componentName: string | null;
  componentImageUrl: string | null;
  thresholdPercent: number;
  initialPrice: number;
  lastNotifiedPrice: number;
  currentAveragePrice: number | null;
  createdAt: string;
}
