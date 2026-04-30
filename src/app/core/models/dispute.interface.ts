
export interface Dispute {
    id: string;
    escrow_id: string;
    opened_by: 'Buyer' | 'Seller';
    reason: string;
    status: 'OPEN' | 'UNDER_REVIEW' | 'RESOLVED';
    resolution?: 'ReleasedToSeller' | 'RefundedToBuyer';
    admin_note?: string;
    created_at: string;
    resolved_at?: string;
    // إضافة بيانات العُهدة عشان نعرضها في اللستة
    escrow_title?: string;
    escrow_amount?: number;
    escrow_currency?: string;
}

export interface DisputeMessage {
    id: string;
    dispute_id: string;
    sender_type: 'Buyer' | 'Seller' | 'Admin';
    message: string;
    attachment_url?: string;
    attachment_name?: string;
    created_at: string;
}


export interface OpenDisputePayload {
    escrow_id: string;
    reason: string;
}