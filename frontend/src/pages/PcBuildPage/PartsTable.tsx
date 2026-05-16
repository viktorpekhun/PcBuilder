import { Link } from "react-router-dom";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState, MultiKey, SingleKey } from "./types";
import { MULTI_TYPES, SINGLE_TYPES } from "./types";

const SLOT_TAG: Record<SingleKey | MultiKey, string> = {
    cpu: "CPU",
    gpu: "GPU",
    motherboard: "M/B",
    rams: "RAM",
    ssds: "SSD",
    hdds: "HDD",
    powerSupply: "PSU",
    cpuCooler: "CLR",
    pcCase: "CSE",
    fans: "FAN",
};

const MORE_LABEL: Record<MultiKey, string> = {
    rams: "Додати ще модуль RAM",
    ssds: "Додати ще SSD",
    hdds: "Додати ще HDD",
    fans: "Додати ще вентилятор",
};

interface PartsTableProps {
    componentData: ComponentDataState;
    loading: boolean;
    onRemoveSingle: (key: SingleKey) => void;
    onRemoveMulti: (key: MultiKey, componentId: string) => void;
    onAdjustQty: (key: MultiKey, componentId: string, change: number) => void;
}

function fmt(n: number): string {
    if (!n) return "0";
    return Math.round(n).toLocaleString("uk-UA");
}

function slotHint(slot: SingleKey | MultiKey): string {
    switch (slot) {
        case "cpu": return "Виберіть процесор — він задає сокет та тип RAM";
        case "gpu": return "Опційно для систем з iGPU";
        case "motherboard": return "Має відповідати сокету CPU";
        case "rams": return "DDR4 або DDR5 залежно від платформи";
        case "ssds": return "Рекомендовано хоча б один NVMe";
        case "hdds": return "Опційно · масивне сховище";
        case "powerSupply": return "Підбирайте після CPU + GPU";
        case "cpuCooler": return "Tower або AIO ≤ 165 мм";
        case "pcCase": return "Має відповідати форм-фактору МП";
        case "fans": return "Опційно · корпус зазвичай має 1–3";
    }
}

export default function PartsTable({
    componentData, loading,
    onRemoveSingle, onRemoveMulti, onAdjustQty,
}: PartsTableProps) {
    return (
        <div className={styles.parts}>
            <div className={styles.partsHead}>
                <span>SLOT</span>
                <span></span>
                <span>COMPONENT</span>
                <span>STORE</span>
                <span className={styles.r}>SUBTOTAL · ₴</span>
                <span></span>
                <span></span>
            </div>

            {/* Single components */}
            {SINGLE_TYPES.map(({ key, urlType, label, buttonLabel }) => {
                const data = componentData[key];
                const tag = SLOT_TAG[key];

                if (loading && !data) {
                    return (
                        <div key={key} className={styles.rowSlot}>
                            <div className={styles.slotTag}>[{tag}]</div>
                            <div className={`${styles.thumbMat} ${styles.thumbMatEmpty}`}>
                                <span className={styles.thumbPh}>...</span>
                            </div>
                            <div className={styles.loading}>Завантаження...</div>
                            <div /><div /><div /><div />
                        </div>
                    );
                }

                if (!data) {
                    return (
                        <Link key={key} to={`/components/${urlType}`} className={styles.rowSlot}>
                            <div className={`${styles.slotTag} ${styles.slotTagEmpty}`}>[{tag}]</div>
                            <div className={`${styles.thumbMat} ${styles.thumbMatEmpty}`}>
                                <span className={styles.thumbPh}>—</span>
                            </div>
                            <div>
                                <div className={`${styles.rowName} ${styles.rowNameEmpty}`}>
                                    <span className={styles.addCue}>＋</span>
                                    Додати {buttonLabel.toLowerCase()}
                                </div>
                                <div className={styles.rowSpec}>
                                    <span>{slotHint(key)}</span>
                                </div>
                            </div>
                            <div />
                            <div className={`${styles.rowPrice} ${styles.rowPriceEmpty}`}>—</div>
                            <div />
                            <div />
                        </Link>
                    );
                }

                const offerPrice = data.selectedOffer?.price ?? data.averagePrice ?? 0;

                return (
                    <div key={key} className={styles.rowSlot}>
                        <div className={styles.slotTag}>[{tag}]</div>
                        <Link to={`/components/${urlType}/${data.id}`} className={styles.thumbMat}>
                            {data.photoUrl
                                ? <img src={data.photoUrl} alt={data.name} />
                                : <span className={styles.thumbPh}>{tag}</span>}
                        </Link>
                        <Link to={`/components/${urlType}/${data.id}`} style={{ minWidth: 0, textDecoration: "none", color: "inherit" }}>
                            <div className={styles.rowName} title={data.name}>{data.name}</div>
                            <div className={styles.rowSpec}>
                                <span>{label}</span>
                            </div>
                        </Link>
                        <div className={styles.rowStore}>
                            {data.selectedOffer?.storeName && (
                                <>
                                    <span className={styles.storeName}>{data.selectedOffer.storeName}</span>
                                    <span>₴ {fmt(offerPrice)}</span>
                                </>
                            )}
                        </div>
                        <div className={styles.rowPrice}>
                            <span className={styles.ccy}>₴</span>{fmt(offerPrice)}
                        </div>
                        <div />
                        <button
                            className={styles.rowX}
                            title="Видалити"
                            onClick={(e) => { e.preventDefault(); onRemoveSingle(key); }}
                        >
                            ×
                        </button>
                    </div>
                );
            })}

            {/* Multi components */}
            {MULTI_TYPES.map(({ key, urlType, label, buttonLabel }) => {
                const items = componentData[key];
                const tag = SLOT_TAG[key];

                if (items.length === 0) {
                    return (
                        <Link key={key} to={`/components/${urlType}`} className={styles.rowSlot}>
                            <div className={`${styles.slotTag} ${styles.slotTagEmpty}`}>[{tag}]</div>
                            <div className={`${styles.thumbMat} ${styles.thumbMatEmpty}`}>
                                <span className={styles.thumbPh}>—</span>
                            </div>
                            <div>
                                <div className={`${styles.rowName} ${styles.rowNameEmpty}`}>
                                    <span className={styles.addCue}>＋</span>
                                    Додати {buttonLabel.toLowerCase()}
                                </div>
                                <div className={styles.rowSpec}>
                                    <span>{slotHint(key)}</span>
                                </div>
                            </div>
                            <div />
                            <div className={`${styles.rowPrice} ${styles.rowPriceEmpty}`}>—</div>
                            <div />
                            <div />
                        </Link>
                    );
                }

                const total = items.length;

                return (
                    <div key={key}>
                        {items.map((item, i) => {
                            const subtotal = item.price * item.quantity;
                            return (
                                <div key={item.componentId} className={styles.rowSlot}>
                                    <div>
                                        <div className={styles.slotTag}>[{tag}]</div>
                                        {total > 1 && (
                                            <div className={styles.slotMulti}>· {i + 1} OF {total}</div>
                                        )}
                                    </div>
                                    <Link to={`/components/${urlType}/${item.componentId}`} className={styles.thumbMat}>
                                        {item.component.photoUrl
                                            ? <img src={item.component.photoUrl} alt={item.component.name} />
                                            : <span className={styles.thumbPh}>{tag}</span>}
                                    </Link>
                                    <Link to={`/components/${urlType}/${item.componentId}`} style={{ minWidth: 0, textDecoration: "none", color: "inherit" }}>
                                        <div className={styles.rowName} title={item.component.name}>
                                            {item.component.name}
                                            {item.quantity > 1 && <span className={styles.rowQty}>× {item.quantity}</span>}
                                        </div>
                                        <div className={styles.rowSpec}>
                                            <span>{label}</span>
                                        </div>
                                    </Link>
                                    <div className={styles.rowStore}>
                                        {item.storeName && (
                                            <>
                                                <span className={styles.storeName}>{item.storeName}</span>
                                                <span>₴ {fmt(item.price)}</span>
                                            </>
                                        )}
                                    </div>
                                    <div className={styles.rowPrice}>
                                        <span className={styles.ccy}>₴</span>{fmt(subtotal)}
                                    </div>
                                    <div className={styles.qtyControl}>
                                        <button className={styles.qtyBtn}
                                            onClick={() => onAdjustQty(key, item.componentId, 1)}>▲</button>
                                        <span className={styles.qtyDisplay}>{item.quantity}</span>
                                        <button className={styles.qtyBtn}
                                            onClick={() => onAdjustQty(key, item.componentId, -1)}>▼</button>
                                    </div>
                                    <button
                                        className={styles.rowX}
                                        title="Видалити"
                                        onClick={() => onRemoveMulti(key, item.componentId)}
                                    >
                                        ×
                                    </button>
                                </div>
                            );
                        })}
                        <Link to={`/components/${urlType}`} className={styles.addRow}>
                            <span className={styles.aGly}>[{tag}]</span>
                            <span className={styles.aPlus}>＋</span>
                            <span>{MORE_LABEL[key]}</span>
                            <span className={styles.aHint}>BROWSE →</span>
                        </Link>
                    </div>
                );
            })}
        </div>
    );
}

