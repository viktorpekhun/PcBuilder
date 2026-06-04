import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState, MultiKey, SingleKey } from "./types";
import { MULTI_TYPES, SINGLE_TYPES } from "./types";
import { getRowSpec } from "./rowSpecs";
import RowAlertButton from "./RowAlertButton";
import { SLOT_TAG } from "./constants";

interface PartsTableProps {
    componentData: ComponentDataState;
    loading: boolean;
    selectedRow: string | null;
    onSelectRow: (id: string | null) => void;
    onRemoveSingle: (key: SingleKey) => void;
    onRemoveMulti: (key: MultiKey, componentId: string) => void;
    onAdjustQty: (key: MultiKey, componentId: string, change: number) => void;
}

function fmt(n: number): string {
    if (!n) return "0";
    return Math.round(n).toLocaleString("uk-UA");
}

export default function PartsTable({
    componentData, loading,
    selectedRow, onSelectRow,
    onRemoveSingle, onRemoveMulti, onAdjustQty,
}: PartsTableProps) {
    const { t } = useTranslation();

    const toggleSelect = (id: string) => {
        onSelectRow(selectedRow === id ? null : id);
    };

    return (
        <div className={styles.parts}>
            <div className={styles.partsHead}>
                <span>{t("pcBuildPage.partsTable.slot")}</span>
                <span></span>
                <span>{t("pcBuildPage.partsTable.component")}</span>
                <span>{t("pcBuildPage.partsTable.store")}</span>
                <span className={styles.r}>{t("pcBuildPage.partsTable.subtotal")}</span>
                <span></span>
                <span></span>
            </div>

            {/* Single components */}
            {SINGLE_TYPES.map(({ key, urlType, apiType }) => {
                const data = componentData[key];
                const tag = SLOT_TAG[key];
                const label = t(`pcBuildPage.componentTypes.${key}.label`);
                const buttonLabel = t(`pcBuildPage.componentTypes.${key}.buttonLabel`);
                const slotHint = t(`pcBuildPage.partsTable.slotHints.${key}`);

                if (loading && !data) {
                    return (
                        <div key={key} className={styles.rowSlot}>
                            <div className={styles.slotTag}>[{tag}]</div>
                            <div className={`${styles.thumbMat} ${styles.thumbMatEmpty}`}>
                                <span className={styles.thumbPh}>...</span>
                            </div>
                            <div className={styles.loading}>{t("pcBuildPage.partsTable.loading")}</div>
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
                                    {t("pcBuildPage.partsTable.addComponent", { label: buttonLabel.toLowerCase() })}
                                </div>
                                <div className={styles.rowSpec}>
                                    <span>{slotHint}</span>
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
                const rowId = key;
                const isSelected = selectedRow === rowId;

                return (
                    <div
                        key={key}
                        className={`${styles.rowSlot} ${isSelected ? styles.rowSlotSel : ""}`}
                        onClick={() => toggleSelect(rowId)}
                    >
                        <div className={styles.slotTag}>[{tag}]</div>
                        <Link to={`/components/${urlType}/${data.id}`} className={styles.thumbMat}>
                            {data.photoUrl
                                ? <img src={data.photoUrl} alt={data.name} />
                                : <span className={styles.thumbPh}>{tag}</span>}
                        </Link>
                        <div style={{ minWidth: 0 }}>
                            <Link to={`/components/${urlType}/${data.id}`} style={{ minWidth: 0, textDecoration: "none", color: "inherit", display: "block" }}>
                                <div className={styles.rowName} title={data.name}>{data.name}</div>
                                <div className={styles.rowSpec}>
                                    {(() => {
                                        const specs = getRowSpec(key, data as unknown as Record<string, unknown>);
                                        if (specs.length === 0) return <span>{label}</span>;
                                        return specs.map((s, idx) => (
                                            <span key={idx} style={{ display: "inline-flex", alignItems: "center", gap: 8 }}>
                                                {idx > 0 && <span className={styles.dot}>/</span>}
                                                <span>{s}</span>
                                            </span>
                                        ));
                                    })()}
                                </div>
                            </Link>
                            <div className={styles.rowTools}>
                                <Link
                                    to={`/components/${urlType}`}
                                    className={styles.rowToolBtn}
                                    onClick={(e) => e.stopPropagation()}
                                >
                                    <span className={styles.toolGly}>⇋</span> {t("pcBuildPage.partsTable.altBtn")}
                                </Link>
                                <RowAlertButton componentId={data.id} componentType={apiType} />
                            </div>
                        </div>
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
                            title={t("pcBuildPage.partsTable.removeTitle")}
                            onClick={(e) => { e.preventDefault(); e.stopPropagation(); onRemoveSingle(key); }}
                        >
                            ×
                        </button>
                    </div>
                );
            })}

            {/* Multi components */}
            {MULTI_TYPES.map(({ key, urlType, apiType }) => {
                const items = componentData[key];
                const tag = SLOT_TAG[key];
                const label = t(`pcBuildPage.componentTypes.${key}.label`);
                const buttonLabel = t(`pcBuildPage.componentTypes.${key}.buttonLabel`);
                const slotHint = t(`pcBuildPage.partsTable.slotHints.${key}`);
                const addMoreLabel = t(`pcBuildPage.partsTable.addMore.${key}`);

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
                                    {t("pcBuildPage.partsTable.addComponent", { label: buttonLabel.toLowerCase() })}
                                </div>
                                <div className={styles.rowSpec}>
                                    <span>{slotHint}</span>
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
                            const rowId = `${key}:${item.componentId}`;
                            const isSelected = selectedRow === rowId;
                            return (
                                <div
                                    key={item.componentId}
                                    className={`${styles.rowSlot} ${isSelected ? styles.rowSlotSel : ""}`}
                                    onClick={() => toggleSelect(rowId)}
                                >
                                    <div>
                                        <div className={styles.slotTag}>[{tag}]</div>
                                        {total > 1 && (
                                            <div className={styles.slotMulti}>
                                                {t("pcBuildPage.partsTable.ofCount", { index: i + 1, total })}
                                            </div>
                                        )}
                                    </div>
                                    <Link to={`/components/${urlType}/${item.componentId}`} className={styles.thumbMat}>
                                        {item.component.photoUrl
                                            ? <img src={item.component.photoUrl} alt={item.component.name} />
                                            : <span className={styles.thumbPh}>{tag}</span>}
                                    </Link>
                                    <div style={{ minWidth: 0 }}>
                                        <Link to={`/components/${urlType}/${item.componentId}`} style={{ minWidth: 0, textDecoration: "none", color: "inherit", display: "block" }}>
                                            <div className={styles.rowName} title={item.component.name}>
                                                {item.component.name}
                                                {item.quantity > 1 && <span className={styles.rowQty}>× {item.quantity}</span>}
                                            </div>
                                            <div className={styles.rowSpec}>
                                                {(() => {
                                                    const specs = getRowSpec(key, item.component as unknown as Record<string, unknown>);
                                                    if (specs.length === 0) return <span>{label}</span>;
                                                    return specs.map((s, idx) => (
                                                        <span key={idx} style={{ display: "inline-flex", alignItems: "center", gap: 8 }}>
                                                            {idx > 0 && <span className={styles.dot}>/</span>}
                                                            <span>{s}</span>
                                                        </span>
                                                    ));
                                                })()}
                                            </div>
                                        </Link>
                                        <div className={styles.rowTools}>
                                            <Link
                                                to={`/components/${urlType}`}
                                                className={styles.rowToolBtn}
                                                onClick={(e) => e.stopPropagation()}
                                            >
                                                <span className={styles.toolGly}>⇋</span> {t("pcBuildPage.partsTable.altBtn")}
                                            </Link>
                                            <RowAlertButton componentId={item.componentId} componentType={apiType} />
                                        </div>
                                    </div>
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
                                    <div className={styles.qtyControl} onClick={(e) => e.stopPropagation()}>
                                        <button className={styles.qtyBtn}
                                            onClick={(e) => { e.stopPropagation(); onAdjustQty(key, item.componentId, 1); }}>▲</button>
                                        <span className={styles.qtyDisplay}>{item.quantity}</span>
                                        <button className={styles.qtyBtn}
                                            onClick={(e) => { e.stopPropagation(); onAdjustQty(key, item.componentId, -1); }}>▼</button>
                                    </div>
                                    <button
                                        className={styles.rowX}
                                        title={t("pcBuildPage.partsTable.removeTitle")}
                                        onClick={(e) => { e.stopPropagation(); onRemoveMulti(key, item.componentId); }}
                                    >
                                        ×
                                    </button>
                                </div>
                            );
                        })}
                        <Link to={`/components/${urlType}`} className={styles.addRow}>
                            <span className={styles.aGly}>[{tag}]</span>
                            <span className={styles.aPlus}>＋</span>
                            <span>{addMoreLabel}</span>
                            <span className={styles.aHint}>{t("pcBuildPage.partsTable.browseAlt")}</span>
                        </Link>
                    </div>
                );
            })}
        </div>
    );
}
