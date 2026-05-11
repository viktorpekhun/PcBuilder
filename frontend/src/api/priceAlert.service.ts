import { axiosPrivate } from "./axios";
import type { ComponentType } from "../types/component.types";
import type {
  IPriceAlert,
  ICreatePriceAlertRequest,
  IUserPriceAlert,
} from "../types/priceAlert.types";

const PATH = "/priceAlerts";

export const priceAlertService = {
  subscribe: (request: ICreatePriceAlertRequest) =>
    axiosPrivate.post<{ id: string }>(PATH, request),

  unsubscribe: (id: string) =>
    axiosPrivate.delete<{ success: boolean }>(`${PATH}/${id}`),

  getForComponent: async (
    componentId: string,
    componentType: ComponentType
  ): Promise<IPriceAlert | null> => {
    const res = await axiosPrivate.get<IPriceAlert | "">(`${PATH}/for-component`, {
      params: { componentId, componentType },
    });
    if (res.status === 204 || res.data === "") return null;
    return res.data as IPriceAlert;
  },

  getMine: () => axiosPrivate.get<IUserPriceAlert[]>(`${PATH}/mine`),
};
