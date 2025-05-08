<template>
  <div class="flex flex-col min-h-screen checkout-container">
    <NavBar />

    <main
        class="flex-1 max-w-6xl mx-auto py-8 px-6 grid grid-cols-1 lg:grid-cols-3 gap-8"
    >
      <!-- Address & Payment -->
      <section class="col-span-2 bg-gray-50 p-6 rounded-lg shadow-lg">
        <h2 class="text-2xl font-bold text-brown-700 mb-6">Checkout</h2>

        <AddressSelector @address-selected="handleAddressSelection" />

        <PaymentMethodSelector @payment-selected="handlePaymentSelection" />
      </section>

      <!-- Cart Summary -->
      <section class="bg-gray-50 p-6 rounded-lg shadow-lg">
        <h2 class="text-2xl font-bold text-brown-700 mb-6">
          Review Your Cart
        </h2>
        <CartSummary
            :isPayNowEnabled="isPayNowEnabled"
            @pay-now="payNow"
        />
      </section>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import axios from 'axios'
import NavBar from '@/components/NavBar.vue'
import Footer from '@/components/Footer.vue'
import AddressSelector from '@/components/AddressSelector.vue'
import PaymentMethodSelector from '@/components/PaymentMethodSelector.vue'
import CartSummary from '@/components/CartSummary.vue'

const API_BASE_URL = process.env.VUE_APP_API_URL

const selectedAddress = ref(null)
function handleAddressSelection(id) {
  selectedAddress.value = id
}

const paymentMethod = ref(null)
function handlePaymentSelection(method) {
  paymentMethod.value = method
}

const isPayNowEnabled = computed(
    () => !!selectedAddress.value && !!paymentMethod.value
)

const createdOrderId = ref(null)

async function createOrder() {
  const token = localStorage.getItem('authToken')
  const payload = {
    address_id: selectedAddress.value,
    payment_method: paymentMethod.value,
  }
  const resp = await axios.post(
      `${API_BASE_URL}/api/v1/createorder`,
      payload,
      { headers: { Authorization: `Bearer ${token}` } }
  )
  createdOrderId.value = resp.data.id
  return resp.data
}

async function payNow() {
  try {
    if (!createdOrderId.value) {
      await createOrder()
    }
    const token = localStorage.getItem('authToken')
    const payResp = await axios.get(
        `${API_BASE_URL}/api/v1/orders/${createdOrderId.value}/pay/esewa`,
        { headers: { Authorization: `Bearer ${token}` } }
    )

    const form = document.createElement('form')
    form.method = 'POST'
    form.action =
        'https://rc-epay.esewa.com.np/api/epay/main/v2/form'

    Object.entries(payResp.data).forEach(([k, v]) => {
      const inp = document.createElement('input')
      inp.type = 'hidden'
      inp.name = k
      inp.value = v
      form.appendChild(inp)
    })

    document.body.appendChild(form)
    form.submit()
  } catch (err) {
    console.error('Payment initiation failed', err)
    alert('Failed to initiate payment. Please try again.')
  }
}
</script>

<style scoped>
.checkout-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-image: url('@/assets/checkout.webp');
  background-size: cover;
  background-repeat: no-repeat;
  background-position: center;
}
.bg-gray-50 {
  background-color: #fafafa;
}
.text-brown-700 {
  color: #8b4513;
}
/* Loader for address selector */
.loader {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #A0522D;
  border-radius: 50%;
  width: 2rem;
  height: 2rem;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
