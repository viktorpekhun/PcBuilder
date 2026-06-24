import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import en from "../locales/en/translation.json";
import uk from "../locales/uk/translation.json";

i18n
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        resources: {
            en: { translation: en },
            uk: { translation: uk },
        },
        supportedLngs: ["en", "uk"],
        fallbackLng: "uk",
        load: "languageOnly",
        detection: {
            order: ["localStorage", "navigator"],
            lookupLocalStorage: "language",
            caches: ["localStorage"],
        },
        interpolation: {
            escapeValue: false,
        },
    });

export default i18n;
