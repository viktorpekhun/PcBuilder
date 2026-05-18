import { Fragment } from "react";
import { Link } from "react-router-dom";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState, MultiKey, SingleKey } from "./types";
import { MULTI_TYPES, SINGLE_TYPES } from "./types";
import { SLOT_TAG } from "./constants";

const MORE_LABEL: Record<MultiKey, string> = {
    rams: "Додати ще модуль RAM",
    ssds: "Додати ще SSD",
    hdds: "Додати ще HDD",
    fans: "Додати ще вентилятор",
};

function fmt(n: number): string {
    if (!n) return "0";
    return Math.round(n).toLocaleString("uk-UA");
}

interface CardsViewProps {
    componentData: ComponentDataState;
    onRemoveSingle: (key: SingleKey) => void;
    onRemoveMulti: (key: MultiKey, componentId: string) => void;
}

export default function CardsView({ componentData, onRemoveSingle, onRemoveMulti }: CardsViewProps) {
    return (
        <div className={styles.cardsGrid}>
            {SINGLE_TYPES.map(({ key, urlType, label, buttonLabel }) => {
                const data = componentData[key];
                const tag = SLOT_TAG[key];

                if (!data) {
                    return (
                        <Link key={key} to={`/components/${urlType}`} className={`${styles.compCard} ${styles.compCardEmpty}`}>
                            <div className={styles.ccHead}>
                                <span className={styles.slotGly}>[{tag}]</span>
                                <span className={styles.ccLabel}>{label}</span>
                            </div>
                            <div className={`${styles.ccBody} ${styles.ccBodyEmpty}`}>
                                <span className={styles.addCue}>＋</span>
                                <span>Додати {buttonLabel.toLowerCase()}</span>
                            </div>
                        </Link>
                    );
                }

                const offerPrice = data.selectedOffer?.price ?? data.averagePrice ?? 0;

                return (
                    <div key={key} className={styles.compCard}>
                        <div className={styles.ccHead}>
                            <span className={styles.slotGly}>[{tag}]</span>
                            <span className={styles.ccLabel}>{label}</span>
                            <button className={styles.ccX} onClick={() => onRemoveSingle(key)}>×</button>
                        </div>
                        <Link to={`/components/${urlType}/${data.id}`} style={{ textDecoration: "none", color: "inherit" }}>
                            <div className={styles.ccBody}>
                                <div className={styles.ccThumb}>
                                    {data.photoUrl
                                        ? <img src={data.photoUrl} alt={data.name} />
                                        : <span className={styles.thumbPh}>{tag}</span>}
                                </div>
                                <div className={styles.ccInfo}>
                                    <div className={styles.ccName} title={data.name}>{data.name}</div>
                                </div>
                            </div>
                        </Link>
                        <div className={styles.ccFoot}>
                            {data.selectedOffer?.storeName && (
                                <span className={styles.ccStore}>{data.selectedOffer.storeName}</span>
                            )}
                            <span className={styles.ccPrice}>
                                <span className={styles.ccy}>₴</span>{fmt(offerPrice)}
                            </span>
                        </div>
                    </div>
                );
            })}

            {MULTI_TYPES.map(({ key, urlType, label, buttonLabel }) => {
                const items = componentData[key];
                const tag = SLOT_TAG[key];

                if (items.length === 0) {
                    return (
                        <Link key={key} to={`/components/${urlType}`} className={`${styles.compCard} ${styles.compCardEmpty}`}>
                            <div className={styles.ccHead}>
                                <span className={styles.slotGly}>[{tag}]</span>
                                <span className={styles.ccLabel}>{label}</span>
                            </div>
                            <div className={`${styles.ccBody} ${styles.ccBodyEmpty}`}>
                                <span className={styles.addCue}>＋</span>
                                <span>Додати {buttonLabel.toLowerCase()}</span>
                            </div>
                        </Link>
                    );
                }

                return (
                    <Fragment key={key}>
                        {items.map((item) => (
                            <div key={item.componentId} className={styles.compCard}>
                                <div className={styles.ccHead}>
                                    <span className={styles.slotGly}>[{tag}]</span>
                                    <span className={styles.ccLabel}>
                                        {label}{item.quantity > 1 ? ` × ${item.quantity}` : ""}
                                    </span>
                                    <button className={styles.ccX} onClick={() => onRemoveMulti(key, item.componentId)}>×</button>
                                </div>
                                <Link to={`/components/${urlType}/${item.componentId}`} style={{ textDecoration: "none", color: "inherit" }}>
                                    <div className={styles.ccBody}>
                                        <div className={styles.ccThumb}>
                                            {item.component.photoUrl
                                                ? <img src={item.component.photoUrl} alt={item.component.name} />
                                                : <span className={styles.thumbPh}>{tag}</span>}
                                        </div>
                                        <div className={styles.ccInfo}>
                                            <div className={styles.ccName} title={item.component.name}>{item.component.name}</div>
                                        </div>
                                    </div>
                                </Link>
                                <div className={styles.ccFoot}>
                                    {item.storeName && (
                                        <span className={styles.ccStore}>{item.storeName}</span>
                                    )}
                                    <span className={styles.ccPrice}>
                                        <span className={styles.ccy}>₴</span>{fmt(item.price * item.quantity)}
                                    </span>
                                </div>
                            </div>
                        ))}
                        <Link key={`${key}:add`} to={`/components/${urlType}`} className={`${styles.compCard} ${styles.compCardEmpty}`}>
                            <div className={styles.ccHead}>
                                <span className={styles.slotGly}>[{tag}]</span>
                                <span className={styles.ccLabel}>{label}</span>
                            </div>
                            <div className={`${styles.ccBody} ${styles.ccBodyEmpty}`}>
                                <span className={styles.addCue}>＋</span>
                                <span>{MORE_LABEL[key]}</span>
                            </div>
                        </Link>
                    </Fragment>
                );
            })}
        </div>
    );
}
