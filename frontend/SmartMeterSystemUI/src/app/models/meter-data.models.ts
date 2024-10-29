export interface MeterData {
    uuid: string;
    serialNumber: string;
    readingTime: Date;
    lastIndex: number;
    voltageValue: number;
    currentValue: number;
}