<template>
  <transition name="fade">
    <div
        v-if="order"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 backdrop-blur-sm p-4"
    >
      <div
          class="bg-white rounded-2xl shadow-2xl overflow-y-auto max-h-[90vh] w-full max-w-3xl relative animate-fadeIn"
      >
        <!-- Close Button -->
        <button
            @click="$emit('close')"
            class="absolute top-4 right-4 text-gray-500 hover:text-red-600 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
               viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <div class="p-8 space-y-6">
          <!-- Header -->
          <h2 class="text-3xl font-extrabold text-gray-800 text-center uppercase tracking-wider">
            Custom Order #{{ order.id }} Details
          </h2>

          <!-- Original Order -->
          <section class="space-y-4">
            <h3 class="text-xl font-semibold text-gray-700">Original Order</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
              <div class="flex"><span class="w-32 font-medium">User ID:</span><span>{{ order.user_id }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Artisan ID:</span><span>{{ order.artisan_id }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Description:</span><span>{{ order.description }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Budget:</span><span>{{ formatMoney(order.budget) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Deadline:</span><span>{{ formatDate(order.deadline) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Status:</span><span>{{ capitalize(order.status) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Created:</span><span>{{ formatDateTime(order.created_at) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Updated:</span><span>{{ formatDateTime(order.updated_at) }}</span></div>
            </div>
          </section>

          <!-- Finalized Details -->
          <section class="space-y-4">
            <h3 class="text-xl font-semibold text-gray-700">Finalized Details</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
              <div class="flex"><span class="w-32 font-medium">Final Description:</span><span>{{ order.final_description || '—' }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Final Price:</span><span>{{ formatMoney(order.final_price) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Final Deadline:</span><span>{{ formatDate(order.final_deadline) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Finalized At:</span><span>{{ formatDateTime(order.finalized_at) }}</span></div>
            </div>
            <div v-if="order.attachment_path" class="mt-4">
              <h4 class="font-semibold text-gray-700 mb-2">Attachment</h4>
              <a :href="`${apiBaseUrl}/storage/${order.attachment_path}`" target="_blank">
                <img
                    :src="`${apiBaseUrl}/storage/${order.attachment_path}`"
                    alt="Attachment"
                    class="w-full max-h-60 object-contain rounded-lg border border-gray-200 hover:shadow-lg transition-shadow"
                />
              </a>
            </div>
          </section>

          <!-- Payment Info -->
          <section v-if="order.payment" class="space-y-4">
            <h3 class="text-xl font-semibold text-gray-700">Payment Info</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
              <div class="flex"><span class="w-32 font-medium">Payment ID:</span><span>{{ order.payment.id }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Amount:</span><span>₹ {{ order.payment.amount }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Method:</span><span>{{ order.payment.payment_method }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Transaction ID:</span><span>{{ order.payment.transaction_id }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Status:</span><span>{{ capitalize(order.payment.status) }}</span></div>
              <div class="flex"><span class="w-32 font-medium">Paid At:</span><span>{{ formatDateTime(order.payment.created_at) }}</span></div>
            </div>
          </section>

          <!-- eSewa Button (only for customers!) -->
          <div
              v-if="isCustomer && order.status === 'finalized' && (!order.payment || order.payment.status === 'pending')"
              class="flex justify-end"
          >
            <button
                @click="initiatePayment"
                :disabled="loading"
                class="bg-green-600 hover:bg-green-700 text-white px-6 py-3 rounded-md text-sm font-semibold transition-all transform hover:-translate-y-0.5 hover:shadow-lg focus:ring-4 focus:outline-none focus:ring-green-400 disabled:opacity-50"
            >
              <span v-if="loading" class="loader w-4 h-4 mr-2 inline-block"></span>
              <span v-else>Initiate eSewa Payment</span>
            </button>
          </div>

          <!-- Toasts -->
          <Toast :toasts="toasts" @close="removeToast" />
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const props = defineProps({
  order: { type: Object, default: null }
})
const emit = defineEmits(['close'])

const apiBaseUrl = process.env.VUE_APP_API_URL
const loading    = ref(false)

// determine userRole from localStorage once
const isCustomer = computed(() => localStorage.getItem('userRole') === 'customer')

// Toast stack
const toasts = reactive([])
function showToast(msg, type = 'error') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(() => removeToast(id), 5000)
}
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id)
  if (i > -1) toasts.splice(i, 1)
}

// Helpers
const capitalize    = s  => s ? s.charAt(0).toUpperCase() + s.slice(1) : ''
const formatDate    = d  => d ? new Date(d).toLocaleDateString() : '—'
const formatDateTime= dt => dt ? new Date(dt).toLocaleString() : '—'
const formatMoney   = v  => `$${(parseFloat(v) || 0).toFixed(2)}`

// eSewa logic unchanged
async function initiatePayment() {
  loading.value = true
  try {
    const token = localStorage.getItem('authToken')
    const resp  = await axios.get(
        `${apiBaseUrl}/api/v1/custom-orders/${props.order.id}/pay/esewa`,
        { headers: { Authorization: `Bearer ${token}` } }
    )
    const params = resp.data
    if (!params?.amount) throw new Error('Missing payment data')

    const form = document.createElement('form')
    form.method = 'POST'
    form.action = 'https://rc-epay.esewa.com.np/api/epay/main/v2/form'
    Object.entries(params).forEach(([k,v]) => {
      const input = document.createElement('input')
      input.type  = 'hidden'
      input.name  = k
      input.value = v
      form.appendChild(input)
    })
    document.body.appendChild(form)
    form.submit()
  } catch (err) {
    console.error(err)
    showToast(err.response?.data?.message || err.message || 'Payment initiation failed', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to           { opacity: 0; }
@keyframes fadeIn { 0% { opacity:0; transform: scale(0.95) } 100% { opacity:1; transform: scale(1) } }
.animate-fadeIn { animation: fadeIn 0.3s ease forwards; }

.loader {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #16a34a;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
