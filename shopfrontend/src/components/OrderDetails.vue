<template>
  <transition name="fade">
    <div
        v-if="order"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 backdrop-blur-sm p-4 no-scroll"
    >
      <div
          class="bg-[#1a1a1a] text-[#C8B280] rounded-2xl shadow-2xl overflow-y-auto max-h-[90vh] w-full max-w-3xl relative animate-fadeIn p-8 space-y-6 border border-[#333]"
      >
        <!-- Close -->
        <button
            @click="close"
            class="absolute top-4 right-4 text-[#888] hover:text-red-400 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
               viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <!-- Header -->
        <h2 class="text-2xl font-bold text-center uppercase tracking-wider text-[#F5DEB3]">
          Order #{{ order.id }} Details
        </h2>

        <!-- Basic Info -->
        <section class="space-y-2">
          <h3 class="text-lg font-semibold text-[#F5DEB3]">Order Info</h3>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="flex"><span class="w-32 font-medium">Date:</span><span>{{ formatDate(order.orderDate) }}</span></div>
            <div class="flex"><span class="w-32 font-medium">Status:</span><span :class="statusClass(order.orderStatus)" class="capitalize font-semibold">{{ order.orderStatus }}</span></div>
            <div class="flex"><span class="w-32 font-medium">Paid:</span><span>{{ order.isPaid ? 'Yes' : 'No' }}</span></div>
            <div class="flex"><span class="w-32 font-medium">Claim Code:</span><span>{{ order.claimCode }}</span></div>
            <div class="flex"><span class="w-32 font-medium">Pickup By:</span><span>{{ formatDate(order.pickupDeadline) }}</span></div>
          </div>
        </section>

        <!-- Price Summary -->
        <section class="text-sm bg-[#2a2a2a] p-4 rounded-md mt-2">
          <div class="flex justify-between mb-1">
            <span>Subtotal:</span>
            <span>{{ formatMoney(subtotal) }}</span>
          </div>
          <div class="flex justify-between mb-1 text-yellow-400">
            <span>Discount ({{ order.appliedDiscounts || 'none' }}):</span>
            <span>-{{ formatMoney(order.discountAmount) }}</span>
          </div>
          <div class="flex justify-between font-bold mt-2 text-[#F5DEB3]">
            <span>Total:</span>
            <span>{{ formatMoney(order.totalPrice) }}</span>
          </div>
        </section>

        <!-- Cancel Button -->
        <div v-if="order.orderStatus === 'pending'" class="text-right">
          <button
              @click="cancelOrder"
              :disabled="loading"
              class="mt-4 px-6 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md font-semibold transition disabled:opacity-50"
          >
            <span v-if="!loading">Cancel Order</span>
            <span v-else>Processing...</span>
          </button>
        </div>

        <!-- Products -->
        <section class="space-y-2">
          <h3 class="text-lg font-semibold text-[#F5DEB3]">Ordered Books</h3>
          <ul class="divide-y divide-[#333]">
            <li
                v-for="item in order.items"
                :key="item.bookId"
                class="py-4 flex items-center space-x-4"
            >
              <img
                  :src="resolveImage(item.image)"
                  alt="Book"
                  class="w-12 h-12 rounded shadow object-cover"
              />
              <div class="flex-1">
                <p class="font-semibold">{{ item.title }}</p>
                <p class="text-sm">
                  {{ formatMoney(item.unitPrice) }} × {{ item.quantity }}
                  <span v-if="item.discountApplied" class="ml-2 italic text-yellow-400">(-{{ item.discountApplied }} off)</span>
                </p>
              </div>
              <button
                  v-if="order.orderStatus === 'claimed'"
                  @click="$emit('review', item)"
                  class="text-sm text-[#FFD700] hover:underline transition ml-2"
              >
                Review
              </button>
            </li>
          </ul>
        </section>

        <!-- Toasts -->
        <Toast :toasts="toasts" @close="removeToast" />
      </div>
    </div>
  </transition>
</template>

<script setup>
import { defineProps, defineEmits, reactive, ref, computed } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const props = defineProps({
  order: { type: Object, required: true }
})
const emit = defineEmits(['close', 'review'])

const loading = ref(false)
const baseUri = process.env.VUE_APP_API_URL || ''

// Toast logic
const toasts = reactive([])
function showToast(msg, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(() => removeToast(id), 5000)
}
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id)
  if (i !== -1) toasts.splice(i, 1)
}

// Cancel order
async function cancelOrder() {
  try {
    loading.value = true
    await axios.put(`${baseUri}/api/order/${props.order.id}/cancel`, {}, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    })
    showToast('Order cancelled successfully.', 'success')
    props.order.orderStatus = 'cancelled'
  } catch (err) {
    console.error(err)
    showToast('Failed to cancel order.', 'error')
  } finally {
    loading.value = false
  }
}

function close() {
  document.body.classList.remove('no-scroll')
  emit('close')
}

const formatDate = d => d ? new Date(d).toLocaleDateString() : '—'
const formatMoney = v => `$${(parseFloat(v) || 0).toFixed(2)}`
const statusClass = status => ({
  'text-yellow-400': status === 'pending',
  'text-green-400': status === 'delivered',
  'text-red-500': status === 'cancelled',
  'text-blue-400': status === 'claimed'
})
const resolveImage = path => {
  if (!path) return '/fallback.png'
  return path.startsWith('http') ? path : `${baseUri.replace(/\/$/, '')}${path.startsWith('/') ? '' : '/'}${path}`
}

// Subtotal before discount
const subtotal = computed(() =>
    props.order.items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
)
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
.animate-fadeIn {
  animation: fadeIn 0.3s ease forwards;
}
.no-scroll {
  overflow: hidden;
}
</style>
