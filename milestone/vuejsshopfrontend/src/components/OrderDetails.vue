<template>
  <transition name="fade">
    <div
        v-if="order"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 backdrop-blur-sm p-4 no-scroll"
    >
      <div
          class="bg-white rounded-2xl shadow-2xl overflow-y-auto max-h-[90vh] w-full max-w-3xl relative animate-fadeIn p-8 space-y-6"
      >
        <!-- Close -->
        <button
            @click="close"
            class="absolute top-4 right-4 text-gray-500 hover:text-red-600 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
               viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <!-- Header -->
        <h2 class="text-3xl font-extrabold text-gray-800 text-center uppercase tracking-wider">
          Order #{{ order.id }} Details
        </h2>

        <!-- Basic Info -->
        <section class="space-y-2">
          <h3 class="text-xl font-semibold text-gray-700">Order Info</h3>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
            <div class="flex">
              <span class="w-32 font-medium">Date:</span>
              <span>{{ formatDate(order.created_at) }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Status:</span>
              <span :class="statusClass(order.status)" class="capitalize font-semibold">
                {{ order.status }}
              </span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Payment:</span>
              <span>{{ order.payment_method }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Total:</span>
              <span>{{ formatMoney(order.total_price) }}</span>
            </div>
          </div>
        </section>

        <!-- Delivery Address -->
        <section v-if="order.address" class="space-y-2">
          <h3 class="text-xl font-semibold text-gray-700">Delivery Address</h3>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
            <div class="flex">
              <span class="w-32 font-medium">Label:</span>
              <span>{{ order.address.label || '—' }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Street:</span>
              <span>{{ order.address.street }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">City:</span>
              <span>{{ order.address.city }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">State:</span>
              <span>{{ order.address.state }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Postal Code:</span>
              <span>{{ order.address.postal_code }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Country:</span>
              <span>{{ order.address.country }}</span>
            </div>
          </div>
        </section>

        <!-- Payment Info -->
        <section v-if="order.payment" class="space-y-2">
          <h3 class="text-xl font-semibold text-gray-700">Payment Info</h3>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-gray-800">
            <div class="flex">
              <span class="w-32 font-medium">Payment ID:</span>
              <span>{{ order.payment.id }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Amount:</span>
              <span>Nrs {{order.payment.amount}}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Status:</span>
              <span>{{ order.payment.status }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">Txn ID:</span>
              <span>{{ order.payment.transaction_id }}</span>
            </div>
            <div class="flex">
              <span class="w-32 font-medium">At:</span>
              <span>{{ formatDateTime(order.payment.created_at) }}</span>
            </div>
          </div>

          <!-- Reinitiate Payment -->
          <div v-if="order.payment.status === 'pending'" class="mt-4 text-right">
            <button
                @click="reinitiatePayment"
                :disabled="loading"
                class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md text-sm font-semibold transition transform hover:-translate-y-0.5 disabled:opacity-50"
            >
              <span v-if="loading" class="loader w-4 h-4 inline-block mr-2 align-middle"></span>
              <span v-else>Reinitiate Payment</span>
            </button>
          </div>
        </section>

        <!-- Products -->
        <section class="space-y-2">
          <h3 class="text-xl font-semibold text-gray-700">Products</h3>
          <ul class="divide-y divide-gray-200">
            <li
                v-for="prod in order.products"
                :key="prod.id"
                class="py-4 flex items-center space-x-4"
            >
              <img
                  :src="prod.images?.[0]"
                  alt="Product"
                  class="w-12 h-12 rounded shadow object-cover"
              />
              <div class="flex-1">
                <p class="font-medium">{{ prod.name }}</p>
                <p class="text-gray-600 text-sm">
                  {{ formatMoney(prod.price) }} × {{ prod.pivot.quantity }}
                  <span class="ml-2 italic text-gray-500">({{ prod.pivot.status }})</span>
                </p>
                <p v-if="order.status === 'delivered'" class="mt-1">
                  <button
                      @click="$emit('write-review', prod)"
                      class="text-blue-600 hover:text-blue-800 underline text-sm"
                  >Write a Review</button>
                </p>
              </div>
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
import { ref, reactive } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const props = defineProps({
  order: { type: Object, required: true }
})
const emit = defineEmits(['close', 'write-review'])

const apiBaseUrl = process.env.VUE_APP_API_URL
const loading = ref(false)

// Toast stack
const toasts = reactive([])
function showToast(msg, type = 'error') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(() => {
    const idx = toasts.findIndex(t => t.id === id)
    if (idx !== -1) toasts.splice(idx, 1)
  }, 5000)
}
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}

// Close modal
function close() {
  document.body.classList.remove('no-scroll')
  emit('close')
}

// Formatters
const formatDate     = d => d ? new Date(d).toLocaleDateString() : '—'
const formatDateTime = d => d ? new Date(d).toLocaleString()    : '—'
const formatMoney    = v => `$${(parseFloat(v)||0).toFixed(2)}`
const statusClass    = status => ({
  'text-yellow-600': status === 'pending',
  'text-green-600':  status === 'delivered',
  'text-red-600':    status === 'canceled',
  'text-blue-600':   ['shipped','out for delivery'].includes(status)
})

// Reinitiate payment
async function reinitiatePayment() {
  loading.value = true
  try {
    const resp = await axios.post(
        `${apiBaseUrl}/api/v1/payment/${props.order.id}/reinitiate-payment`
    )
    const params = resp.data
    if (!params?.amount) throw new Error('Missing payment data')

    // build & submit form
    const form = document.createElement('form')
    form.method = 'POST'
    form.action = 'https://rc-epay.esewa.com.np/api/epay/main/v2/form'
    Object.entries(params).forEach(([k,v]) => {
      const inp = document.createElement('input')
      inp.type = 'hidden'; inp.name = k; inp.value = v
      form.appendChild(inp)
    })
    document.body.appendChild(form)
    form.submit()
  } catch (err) {
    console.error(err)
    showToast(err.response?.data?.message || err.message || 'Failed to reinitiate payment')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95) }
  to   { opacity: 1; transform: scale(1)    }
}
.animate-fadeIn { animation: fadeIn 0.3s ease forwards; }

.no-scroll {
  overflow: hidden;
}

.loader {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #2563EB;
  border-radius: 50%;
  width: 1em;
  height: 1em;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
