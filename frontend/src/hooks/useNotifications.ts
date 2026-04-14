import { useContext } from "react";
import NotificationContext from "../context/NotificationContext";
import type { INotificationContextType } from "../context/NotificationContext";

const useNotifications = (): INotificationContextType => {
    return useContext(NotificationContext);
};

export default useNotifications;
