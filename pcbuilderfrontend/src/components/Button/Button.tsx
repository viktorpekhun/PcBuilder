import styles from "./Button.module.css";

interface ButtonBaseProps {
    variant?: 'primary' | 'secondary' | 'outline-primary' | 'outline-secondary' | 'danger';
    size?: 'sm' | 'md' | 'lg';
    className?: string | undefined;
    children?: React.ReactNode;
}

type ButtonProps =
    | (ButtonBaseProps & React.ButtonHTMLAttributes<HTMLButtonElement> & { href?: undefined })
    | (ButtonBaseProps & React.AnchorHTMLAttributes<HTMLAnchorElement> & { href: string });

export const Button: React.FC<ButtonProps> = ({
    variant = 'primary',
    size = 'md',
    className = '',
    children,
    ...props
}) => {
    const classes = [
        styles.button,
        styles[variant],
        styles[size],
        className
    ].filter(Boolean).join(' ');

    if ('href' in props && props.href !== undefined) {
        return (
            <a className={classes} {...props as React.AnchorHTMLAttributes<HTMLAnchorElement>}>
                {children}
            </a>
        );
    }

    return (
        <button className={classes} {...props as React.ButtonHTMLAttributes<HTMLButtonElement>}>
            {children}
        </button>
    );
};
