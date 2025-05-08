import { ref } from 'vue'

export const showChatOverlay = ref(false)
export const currentChatOrderId = ref(null)

export function openChat(orderId) {
    currentChatOrderId.value = orderId
    showChatOverlay.value = true
}

export function closeChat() {
    currentChatOrderId.value = null
    showChatOverlay.value = false
}
