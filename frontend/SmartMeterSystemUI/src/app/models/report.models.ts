import { MeterData } from "./meter-data.models";

export interface Report {
    uuid: string;
    requestDate: Date;
    status: number;
    content?: string;
    contentDetail?: ContentDetail;
    meterSerialNumber: string;
}

export interface ContentDetail {
    lastIndex: number;
    readingTime: Date;
    serialNumber: string;
    voltageValue: number;
    currentValue: number;
}

export enum FileType {
    EXCEL = 'excel',
    CSV = 'csv',
    TXT = 'txt'
}